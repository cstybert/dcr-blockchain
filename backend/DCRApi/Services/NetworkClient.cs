using System.Text;

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

    public async Task DiscoverNetwork()
    {
        // Query DNS Server for seed nodes
        var seedNodes = await ConnectToDNSServer();

        Console.WriteLine($"Connecting to seed node networks...");
        await Connect(seedNodes);
    }

    private async Task Connect(List<NetworkNode> nodes)
    {
        foreach (var node in nodes) {
            await ConnectToPeerNetwork(node);
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

    public async Task ConnectToPeerNetwork(NetworkNode node)
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

    private async Task<List<NetworkNode>> ConnectToNode(NetworkNode node) {
        var content = GetConnectionContent();
        try {
            var peerResponse = await _httpClient.PostAsync($"{node.URL}/network/connect", content);
            var jsonString = await peerResponse.Content.ReadAsStringAsync();
            var peerNeighbors = _networkSerializer.Deserialize(jsonString);
            return peerNeighbors;
        }
        catch (Exception ex) {
            PrintError(ex);
            return new List<NetworkNode>();
        }
    }

    // Get Blockchain from random neighbour
    public async Task<Blockchain?> GetBlockchain()
    {
            if (ClientNeighbors.Count == 0)
            {
                Console.WriteLine("ClientNeighbours was 0");
                return null;
            }
            Blockchain blockchain;
            int i = 0;
            while (i < 5) {   
                Random r = new Random();
                int neighbourIndex = r.Next(0, ClientNeighbors.Count - 1);
                var neighbour = ClientNeighbors[neighbourIndex];
                try {
                    Console.WriteLine($"Getting : {neighbour.URL}/blockchain/full");
                    HttpResponseMessage res = await _httpClient.GetAsync($"{neighbour.URL}/blockchain/full");
                    string responseContent = await res.Content.ReadAsStringAsync();
                    blockchain = _blockchainSerializer.Deserialize(responseContent);
                }
                catch (Exception ex) {
                    PrintError(ex);
                    continue;
                }
                if (blockchain is not null)
                {
                    return blockchain;
                }
                i++;
            }
            return null;
    }

    // Get Blockchain from random neighbour
    public async Task<GetHeadResponse?> GetHeadFromNeighbour()
    {
        if (ClientNeighbors.Count == 0)
        {
            return null;
        }
        Block head;
        int i = 0;
        while (i < 5) {   
            Random r = new Random();
            int neighbourIndex = r.Next(0, ClientNeighbors.Count - 1);
            var neighbour = ClientNeighbors[neighbourIndex];
            try {
                HttpResponseMessage res = await _httpClient.GetAsync($"{neighbour.URL}/blockchain/head");
                string responseContent = await res.Content.ReadAsStringAsync();
                head = _blockSerializer.Deserialize(responseContent);
            }
            catch (Exception ex) {
                PrintError(ex);
                continue;
            }
            if (head is not null)
            {
                return new GetHeadResponse(head, neighbour);
            }
            i++;
        }
        return null;
    }

    // Get Blockchain from random neighbour
    public async Task<Block?> GetBlock(NetworkNode neighbour, int index)
    {
            Block block;
            try {
                HttpResponseMessage res = await _httpClient.GetAsync($"{neighbour.URL}/blockchain/{index}");
                string responseContent = await res.Content.ReadAsStringAsync();
                block = _blockSerializer.Deserialize(responseContent);
            }
            catch (Exception ex) {
                PrintError(ex);
                return null;
            }
            return block;
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

    public void BroadcastTransaction(string transactionJson) {
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