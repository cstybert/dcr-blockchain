namespace DCR;
public abstract class AbstractNode
{
    public abstract Blockchain Blockchain {get; init;}
    private readonly BlockchainSerializer _blockchainSerializer = new BlockchainSerializer();
    public abstract  NetworkClient NetworkClient {get; init;}
    // miningCTSource is present in all nodes, to allow for the same implementation in resyncing blockchain
    // even if it is not used in other node types than miner.
    protected CancellationTokenSource miningCTSource = new CancellationTokenSource();
    public abstract void HandleTransaction(Transaction tx);
    protected int Id = new Random().Next();

    protected async void ResyncBlockchain(int NumberNeighbours)
    {
        for (int i = 0; i < NumberNeighbours; i++)
        {
            GetHeadResponse? response = await NetworkClient.GetHeadFromNeighbour();
            if (response is null)
            {
                continue;
            }
            Block localHead = Blockchain.GetHead();
            if (response.RemoteBlock.Index == localHead.Index)
            {
                return;
            }
            if ((response.RemoteBlock.Index > Blockchain.GetHead().Index) && response.RemoteBlock.IsValid(Blockchain.Difficulty))
            {   
                Resync(response.Node, response.RemoteBlock);
            }
        }
    }

    protected void Save()
    {
        var blockchainJson = _blockchainSerializer.Serialize(Blockchain);
        using (StreamWriter sw = System.IO.File.CreateText($"blockchain{Id.ToString()}.json"))
        {
            sw.Write(blockchainJson);
        }
    }

    private async void Resync(NetworkNode Node, Block RemoteHead)
    {
        if (!RemoteHead.IsValid(Blockchain.Difficulty))
        {
            return;
        }
        Block localHead = Blockchain.GetHead();
        // possible race condition.
        // this check is done an instant before a new block is added.
        // new block is added before mining task is cancelled
        if ((RemoteHead.Index == localHead.Index + 1) && RemoteHead.PreviousBlockHash == localHead.Hash)
        {
            miningCTSource.Cancel();
            Blockchain.Append(RemoteHead);
            ShareBlock(RemoteHead);
            Save();
            return;
        }

        int LocalChainLength = Blockchain.Chain.Count();
        Blockchain RemoteChain = new Blockchain(Blockchain.Difficulty);
        RemoteChain.Append(RemoteHead);
        for (int i = RemoteHead.Index; i >= 0; i--)
        {
            Block? RemoteBlock = await NetworkClient.GetBlock(Node, i);
            if (RemoteBlock is null || !RemoteChain.IsValid())
            {
                return;
            }
            RemoteChain.Prepend(RemoteBlock);
            int IndexOfPreviousHash = Blockchain.Chain.FindIndex(b => b.Hash == RemoteBlock.PreviousBlockHash);
            // If no block has hash equal to remote previousblockhash request another block
            if (IndexOfPreviousHash == -1)
            {
                continue;
            }
            int LengthOfReplacing = IndexOfPreviousHash + RemoteChain.Chain.Count();
            if (LengthOfReplacing <= LocalChainLength)
            {
                return;
            }
            miningCTSource.Cancel();
            // Delete all blocks from the Index of PreviousBlockHash + 1 and to the end of the chain
            Blockchain.RemoveRange(IndexOfPreviousHash + 1, Blockchain.Chain.Count() - IndexOfPreviousHash + 1);
            Blockchain.Append(RemoteChain.Chain);
            ShareBlock(Blockchain.GetHead());
            Save();
        }
    }

    // Send newly mined block to all neighbours
    protected void ShareBlock(Block block)
    {
        Console.WriteLine($"Broadcasting block {block.Hash}");
        NetworkClient.BroadcastBlock(block);
    }

    public void ReceiveBlock(ShareBlock req)
    {
        if (req.Block.Index <= Blockchain.GetHead().Index || !req.Block.IsValid(Blockchain.Difficulty))
        {
            return;
        }
        Resync(req.Sender, req.Block);
    }
}