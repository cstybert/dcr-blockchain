namespace DCR;
public  class ShareBlock
{
    public Block Block {get; init;}
    public NetworkNode Sender {get; init;}

    public ShareBlock(Block block, NetworkNode sender)
    {
        Block = block;
        Sender = sender;
    }
}