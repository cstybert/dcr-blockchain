namespace DCR;
public abstract class AbstractNode
{
    public Blockchain Blockchain {get; init;}
    private readonly BlockchainSerializer _blockchainSerializer = new BlockchainSerializer();
    public abstract void ReceiveBlock(Block block);
    public abstract void HandleTransaction(Transaction tx);
}