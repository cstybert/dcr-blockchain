namespace DCR;
public  class ShareBlock
{
    public Block Block {get; init;}
    public Node Sender {get; init;}

    public ShareBlock(Block block, Node sender)
    {
        Block = block;
        Sender = sender;
    }
}