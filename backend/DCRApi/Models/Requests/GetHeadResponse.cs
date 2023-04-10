namespace DCR;
public class GetHeadResponse
{
    public Block RemoteBlock {get; init;}
    public NetworkNode Node {get; init;}
    public GetHeadResponse(Block block, NetworkNode node)
    {
        RemoteBlock = block;
        Node = node;
    }
}