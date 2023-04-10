using System.Text;

namespace DCR;

public class NetworkClient : IDisposable
{
    private NetworkSerializer _networkSerializer;
    private HttpClient _httpClient;
    public Node ClientNode {get;}
    public List<Node> ClientNeighbors {get;}

    public NetworkClient(string address, int port)
    {
        _networkSerializer = new NetworkSerializer();
        _httpClient = new HttpClient();
        ClientNode = new Node(address, port);
        ClientNeighbors = new List<Node>();
    }

    public async void DiscoverNetwork()
    {
        // Query DNS Server for seed nodes
        var seedNodes = await ConnectToDNSServer();

        Console.WriteLine($"Connecting to seed node networks...");
        foreach (var node in seedNodes) {
            ConnectToPeerNetwork(node);
        }
    }

    public async Task DisconnectFromNetwork()
    {
        // Disconnect from all neighbors
        foreach (var neighbor in ClientNeighbors) {
            try
            {
                // RemoveNode(neighbor); TODO: Is removing neighbor from the list necessary? The list is wiped anyway..
                await DisconnectFromNode(neighbor);
            }
            catch (Exception ex)
            {
                PrintError(ex);
            }
        }
    }

    private async Task<List<Node>> ConnectToDNSServer()
    {
        Console.WriteLine("Connecting to DNS server...");
        try {
            var dnsResponse = await _httpClient.GetAsync("http://localhost:5000/DNS"); // TODO: Add retry-functionality?
            var responseContent = await dnsResponse.Content.ReadAsStringAsync();
            var seedNodes = _networkSerializer.Deserialize(responseContent);
            return seedNodes;
        }
        catch (Exception ex)
        {
            PrintError(ex);
            return new List<Node>();
        }
    }

    public async void ConnectToPeerNetwork(Node node)
    {
        if (AddNode(node)) {
            var peerNeighbors = await ConnectToNode(node);
            Console.WriteLine($"Connected to node {node.URL}");
            foreach (var neighbor in peerNeighbors) {
                if (AddNode(neighbor)) {
                    await ConnectToNode(neighbor); // TODO: Connect to neighbor's neighbors?
                    Console.WriteLine($"Connected to node {neighbor.URL} (neighbor)");
                }
            }
        }
    }

    private async Task<List<Node>> ConnectToNode(Node node) {
        var content = GetConnectionContent();
        try {
            var peerResponse = await _httpClient.PostAsync($"{node.URL}/network/connect", content);
            var jsonString = await peerResponse.Content.ReadAsStringAsync();
            var peerNeighbors = _networkSerializer.Deserialize(jsonString);
            return peerNeighbors;
        }
        catch (Exception ex) {
            PrintError(ex);
            return new List<Node>();
        }
    }

    private async Task DisconnectFromNode(Node node) {
        var content = GetConnectionContent();
        try {
            await _httpClient.PostAsync($"{node.URL}/network/disconnect", content);
            Console.WriteLine($"Disconnected from node {node.URL}");
        }
        catch (Exception ex) {
            PrintError(ex);
        }
    }

    public async void BroadcastBlock(string blockJson) {
        var content = new StringContent(blockJson, Encoding.UTF8, "application/json");
        foreach (var neighbor in ClientNeighbors) {
            try
            {
                await _httpClient.PostAsync($"{neighbor.URL}/blockchain/block", content);
            }
            catch (Exception ex)
            {
                // TODO: Determine if we should remove unreliable neighbor
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

    public void PrintNeighborList()
    {
        Console.WriteLine($"Neighbor List:");
        foreach (var neighbor in ClientNeighbors) {
            Console.WriteLine(neighbor.URL);
        }
    }

    private static void PrintError(Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex.Message);
        Console.ResetColor();
    }

    private StringContent GetConnectionContent()
    {
        var connectNode = new ConnectNode(ClientNode, ClientNeighbors);
        var nodeJson = _networkSerializer.Serialize(connectNode);
        var content = new StringContent(nodeJson, Encoding.UTF8, "application/json");
        return content;
    }

    // Make sure to timely dispose the HttpClient to avoid waiting for GC
    public void Dispose() {
        _httpClient.Dispose();
    }
}