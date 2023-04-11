namespace DCR;
public class ConnectRequest
{
    public NetworkNode Node {get; init;}
    public List<NetworkNode> Neighbors {get; init;}

    public ConnectRequest(NetworkNode node, List<NetworkNode> neighbors) 
    {
        Node = node;
        Neighbors = neighbors;
    }
}