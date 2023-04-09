using System.Text;

namespace DCR;

public class NetworkClient
{
    private NetworkSerializer _networkSerializer;
    public Node ClientNode {get;}
    public List<Node> ClientNeighbors {get;}

    public NetworkClient(string address, int port)
    {
        _networkSerializer = new NetworkSerializer();
        ClientNode = new Node(address, port);
        ClientNeighbors = new List<Node>();
    }

    public async void DiscoverNetwork()
    {
        using (var client = new HttpClient())
        {
            try
            {
                // Query DNS Server
                Console.WriteLine("Connecting to DNS server...");
                var dnsResponse = await client.GetAsync("http://localhost:5000/DNS");
                var jsonString = await dnsResponse.Content.ReadAsStringAsync();
                var nodes = _networkSerializer.Deserialize(jsonString);

                // Connect to discovered neighbor nodes
                Console.WriteLine($"Connecting to discovered neighbors...");
                foreach (var node in nodes) {
                    ConnectToPeerNetwork(node);
                }

                Console.WriteLine($"Neighbor List:");
                foreach (var neighbor in ClientNeighbors) {
                    Console.WriteLine(neighbor.URL);
                }
            }
            catch (Exception ex)
            {
                PrintError(ex);
            }
        }
    }

    public async Task DisconnectFromNetwork()
    {
        using (var client = new HttpClient())
        {
            try
            {
                // Disconnect from neighbors
                foreach (var neighbor in ClientNeighbors) {
                    // RemoveNode(neighbor);
                    Console.WriteLine("Disconnecting from "+neighbor.URL);
                    await DisconnectFromNode(client, neighbor);
                }
            }
            catch (Exception ex)
            {
                PrintError(ex);
            }
        }
    }

    public async void ConnectToPeerNetwork(Node node) {
        if (!AddNode(node)) {
            Console.WriteLine($"{node.URL} (skipped)");
            return;
        }
        Console.WriteLine($"{node.URL} (added)");

        using (var client = new HttpClient())
        {
            try
            {
                var peerNeighbors = await ConnectToNode(client, node);
                foreach (var neighbor in peerNeighbors) {
                    if (AddNode(neighbor)) {
                        await ConnectToNode(client, neighbor); // TODO: Ignore neighbor's neighbors?
                        Console.WriteLine($"{neighbor.URL} (added neighbor)");
                    }
                }
            }
            catch (Exception ex)
            {
                PrintError(ex);
            }
        }
    }

    private async Task<List<Node>> ConnectToNode(HttpClient client, Node node) {
        var connectNode = new ConnectNode(ClientNode, ClientNeighbors);
        var nodeJson = _networkSerializer.Serialize(connectNode);
        var content = new StringContent(nodeJson, Encoding.UTF8, "application/json");
        var peerResponse = await client.PostAsync($"{node.URL}/network/connect", content);
        var jsonString = await peerResponse.Content.ReadAsStringAsync();
        var peerNeighbors = _networkSerializer.Deserialize(jsonString);
        return peerNeighbors;
    }

    private async Task DisconnectFromNode(HttpClient client, Node node) {
        var connectNode = new ConnectNode(ClientNode, ClientNeighbors);
        var nodeJson = _networkSerializer.Serialize(connectNode);
        var content = new StringContent(nodeJson, Encoding.UTF8, "application/json");
        await client.PostAsync($"{node.URL}/network/disconnect", content);
    }

    public async void Broadcast(string json) {
        using (var client = new HttpClient())
        {
            try
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                foreach (var neighbor in ClientNeighbors) {
                    await client.PostAsync($"{neighbor.URL}/blockchain/block", content);
                }
            }
            catch (Exception ex)
            {
                PrintError(ex);
            }
        }
    }

    public bool AddNode(Node node)
    {
        if (ClientNeighbors.Count < 125 && !(ClientNode.URL == node.URL) && !ClientNeighbors.Any(n => n.URL == node.URL)) {
            ClientNeighbors.Add(node);
            return true;
        }
        return false;
    }

    public void RemoveNode(Node node)
    {
        ClientNeighbors.RemoveAll(n => n.URL == node.URL);
    }

    private static void PrintError(Exception ex) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex.Message);
        Console.ResetColor();
    }
}