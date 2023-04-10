namespace DCR;
public class NetworkNode
{
    public string Address {get; private set;}
    public int Port {get; private set;}

    public NetworkNode(string address, int port) 
    {
        Address = address;
        Port = port;
    }

    public string URL
    {
        get => $"http://{Address}:{Port}";
    }
}