namespace DCR;
public class GetHeadResponse
{
    public Block RemoteBlock {get; init;}
    public Node Node {get; init;}
    public GetHeadResponse(Block block, Node node)
    {
        RemoteBlock = block;
        Node = node;
    }
}