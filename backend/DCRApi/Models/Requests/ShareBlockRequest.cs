namespace DCR;
public  class ShareBlockRequest
{
    public Block Block {get; init;}
    public NetworkNode SourceNode {get; init;}

    public ShareBlockRequest(Block block, NetworkNode sourceNode)
    {
        Block = block;
        SourceNode = sourceNode;
    }
}