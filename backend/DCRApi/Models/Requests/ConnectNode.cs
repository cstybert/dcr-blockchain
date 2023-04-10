namespace DCR;
public class ConnectNode
{
    public NetworkNode Node {get; init;}
    public List<NetworkNode> Neighbors {get; init;}

    public ConnectNode(NetworkNode node, List<NetworkNode> neighbors) 
    {
        Node = node;
        Neighbors = neighbors;
    }
}