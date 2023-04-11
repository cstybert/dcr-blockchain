using System.Text;
using Newtonsoft.Json;

namespace DCR;

public class NetworkClient : IDisposable
{
    private NetworkSerializer _networkSerializer;
    private HttpClient _httpClient;
    public NetworkNode ClientNode {get;}
    public List<NetworkNode> ClientNeighbors {get;}
    private readonly BlockchainSerializer _blockchainSerializer = new BlockchainSerializer();
    private readonly BlockSerializer _blockSerializer = new BlockSerializer();

    public NetworkClient(string address, int port)
    {
        _networkSerializer = new NetworkSerializer();
        _httpClient = new HttpClient();
        ClientNode = new NetworkNode(address, port);
        ClientNeighbors = new List<NetworkNode>();
    }

////////////////////////////////////////////////////////////////////////////////////////////////////////
// Network Connection Functions
////////////////////////////////////////////////////////////////////////////////////////////////////////
    private async Task<List<NetworkNode>> ConnectToDNSServer()
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
            return new List<NetworkNode>();
        }
    }

    public async Task DiscoverNetwork()
    {
        // Query DNS Server for seed nodes
        var seedNodes = await ConnectToDNSServer();

        Console.WriteLine($"Connecting to seed node networks...");
        foreach (var node in seedNodes) {
            await ConnectToNodeNetwork(node);
        }
    }

    // If we don't know the node neighbors, query the node for neighbors
    public async Task ConnectToNodeNetwork(NetworkNode node)
    {
        Console.WriteLine("PEER NETWORK");
        if (AddNode(node)) {
            var peerNeighbors = await ConnectToNode(node);
            foreach (var neighbor in peerNeighbors) {
                if (AddNode(neighbor)) {
                    await ConnectToNode(neighbor); // TODO: Connect to neighbor's neighbors?
                }
            }
            PrintNeighborList();
        }
    }

    // If we already know the node neighbors, no need to fetch
    public async Task ConnectToNodeNetwork(NetworkNode node, List<NetworkNode> nodeNeighbors)
    {
        Console.WriteLine("PEER NETWORK WITH NEIGHBORS");
        if (AddNode(node)) {
            foreach (var neighbor in nodeNeighbors) {
                if (AddNode(neighbor)) {
                    await ConnectToNode(neighbor); // TODO: Connect to neighbor's neighbors?
                }
            }
            PrintNeighborList();
        }
    }


    private async Task<List<NetworkNode>> ConnectToNode(NetworkNode node) {
        var content = GetConnectionContent();
        try {
            var peerResponse = await _httpClient.PostAsync($"{node.URL}/network/connect", content);
            var jsonString = await peerResponse.Content.ReadAsStringAsync();
            var peerNeighbors = _networkSerializer.Deserialize(jsonString);
            Console.WriteLine($"Connected to node {node.URL}");
            return peerNeighbors;
        }
        catch (Exception ex) {
            Console.WriteLine("HERE");
            PrintError(ex);
            Console.WriteLine($"Could not connect to node {node.URL}");
            return new List<NetworkNode>();
        }
    }

    public async Task DisconnectFromNetwork()
    {
        // Disconnect from all neighbors
        foreach (var neighbor in ClientNeighbors) {
            // RemoveNode(neighbor); TODO: Is removing neighbor from the list necessary? The list is wiped anyway..
            await DisconnectFromNode(neighbor);
        }
    }

    private async Task DisconnectFromNode(NetworkNode node) {
        var content = GetConnectionContent();
        try {
            await _httpClient.PostAsync($"{node.URL}/network/disconnect", content);
            Console.WriteLine($"Disconnected from node {node.URL}");
        }
        catch (Exception ex) {
            PrintError(ex);
        }
    }

////////////////////////////////////////////////////////////////////////////////////////////////////////
// Blockchain Communication Functions
////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Get Blockchain from random neighbour
    public async Task<Blockchain?> GetBlockchain(NetworkNode node)
    {
        try
        {
            HttpResponseMessage res = await _httpClient.GetAsync($"{node.URL}/blockchain/full");
            string responseContent = await res.Content.ReadAsStringAsync();
            var blockchain = _blockchainSerializer.Deserialize(responseContent);
            return blockchain;
        }
        catch (Exception ex)
        {
            PrintError(ex);
            return null;
        }
    }

    // Get head block from random neighbour
    public async Task<Block?> GetHead(NetworkNode node)
    {
        try
        {
            HttpResponseMessage res = await _httpClient.GetAsync($"{node.URL}/blockchain/head");
            string responseContent = await res.Content.ReadAsStringAsync();
            var headBlock = _blockSerializer.Deserialize(responseContent);
            return headBlock;
        }
        catch (Exception ex)
        {
            PrintError(ex);
            return null;
        }
    }

    // Get specific block from random neighbour
    public async Task<Block?> GetBlock(NetworkNode node, int index)
    {
        try
        {
            HttpResponseMessage res = await _httpClient.GetAsync($"{node.URL}/blockchain/{index}");
            string responseContent = await res.Content.ReadAsStringAsync();
            var block = _blockSerializer.Deserialize(responseContent);
            return block;
        }
        catch (Exception ex)
        {
            PrintError(ex);
            return null;
        }
    }

    public void BroadcastBlock(Block block) {
        var connectNode = new ShareBlock(block, ClientNode);
        var shareJson = _networkSerializer.Serialize(connectNode);
        var content = new StringContent(shareJson, Encoding.UTF8, "application/json");
        foreach (var neighbor in ClientNeighbors) {
            try
            {
                _httpClient.PostAsync($"{neighbor.URL}/blockchain/block", content);
            }
            catch (Exception ex)
            {
                // TODO: Determine if we should remove unreliable neighbor
                PrintError(ex);
            }
        }
    }

    public void BroadcastTransaction(Transaction Transaction) {
        string transactionJson = JsonConvert.SerializeObject(Transaction);
        var content = new StringContent(transactionJson, Encoding.UTF8, "application/json");
        foreach (var neighbor in ClientNeighbors) {
            try
            {
                _httpClient.PostAsync($"{neighbor.URL}/blockchain/transaction", content);
            }
            catch (Exception ex)
            {
                // TODO: Determine if we should remove unreliable neighbor
                PrintError(ex);
            }
        }
    }

////////////////////////////////////////////////////////////////////////////////////////////////////////
// Helper Functions
////////////////////////////////////////////////////////////////////////////////////////////////////////
    public bool AddNode(NetworkNode node)
    {
        if (ClientNeighbors.Count < 125 && !(ClientNode.URL == node.URL) && !ClientNeighbors.Any(n => n.URL == node.URL)) {
            ClientNeighbors.Add(node);
            return true;
        }
        return false;
    }

    public void RemoveNode(NetworkNode node)
    {
        ClientNeighbors.RemoveAll(n => n.URL == node.URL);
    }

    private StringContent GetConnectionContent()
    {
        var connectNode = new ConnectNode(ClientNode, ClientNeighbors);
        var nodeJson = _networkSerializer.Serialize(connectNode);
        var content = new StringContent(nodeJson, Encoding.UTF8, "application/json");
        return content;
    }

    public List<NetworkNode> GetRandomNeighbors(int numberNeighbors) {
        var random = new Random();   
        var randomNeighbors = ClientNeighbors.OrderBy(n => random.Next()).Take(numberNeighbors).ToList();
        return randomNeighbors;
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

    // Make sure to timely dispose the HttpClient to avoid waiting for GC
    public void Dispose() {
        _httpClient.Dispose();
    }
}