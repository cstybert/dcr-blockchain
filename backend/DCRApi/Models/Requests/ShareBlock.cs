namespace DCR;
public  class ShareBlock
{
    public Block Block {get; init;}
    public NetworkNode SourceNode {get; init;}

    public ShareBlock(Block block, NetworkNode sourceNode)
    {
        Block = block;
        SourceNode = sourceNode;
    }
}