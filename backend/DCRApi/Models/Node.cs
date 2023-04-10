namespace DCR;
public class Node
{
    public string Address {get; private set;}
    public int Port {get; private set;}

    public Node(string address, int port) 
    {
        Address = address;
        Port = port;
    }

    public string URL
    {
        get => $"http://{Address}:{Port}";
    }
}