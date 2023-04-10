namespace DCR;

using Microsoft.Extensions.Options;
using Newtonsoft.Json;
public abstract class AbstractNode
{
    public Blockchain Blockchain {get; init;}
    private readonly BlockchainSerializer _blockchainSerializer = new BlockchainSerializer();
    public  NetworkClient NetworkClient {get; init;}
    // miningCTSource is present in all nodes, to allow for the same implementation in resyncing blockchain
    // even if it is not used in other node types than miner.
    public CancellationTokenSource miningCTSource = new CancellationTokenSource();
    public abstract void HandleTransaction(Transaction tx);
    public abstract void Mine();
    protected int Id = new Random().Next();


    public AbstractNode(NetworkClient networkClient)
    {
        NetworkClient = networkClient;
        if (!System.IO.File.Exists($"blockchain{Id.ToString()}.json"))
        {
            Blockchain? blockchain = NetworkClient.GetBlockchain().Result;
            CancellationToken mineCT = miningCTSource.Token;
            if (blockchain is not null)
            {
                Console.WriteLine("Blockchain is not null");
                Blockchain = blockchain;
                Save();
            }
            else
            {
                Console.WriteLine("Blockchain is null");
                Blockchain = new Blockchain(Settings.Difficulty);
                Blockchain.Initialize(mineCT);
                Save();
            }
        }
        else
        {
            var blockJson = System.IO.File.ReadAllText($"blockchain{Id.ToString()}.json");
            Blockchain = _blockchainSerializer.Deserialize(blockJson);
            ResyncBlockchain(Settings.NumberNeighbours);
            Save();
        }
    }


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
        string blockJson = JsonConvert.SerializeObject(block);
        Console.WriteLine($"Broadcasting block {blockJson}");
        NetworkClient.BroadcastBlock(block);
    }

    public void ReceiveBlock(ShareBlock req)
    {
        lock(Blockchain)
        {
            if (req.Block.Index <= Blockchain.GetHead().Index || !req.Block.IsValid(Blockchain.Difficulty))
            {
                return;
            }
            Resync(req.Sender, req.Block);
        }
    }
}