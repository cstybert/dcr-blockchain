namespace DCR;
public class ConnectNode
{
    public Node Node {get; init;}
    public List<Node> Neighbors {get; init;}

    public ConnectNode(Node node, List<Node> neighbors) 
    {
        Node = node;
        Neighbors = neighbors;
    }
}