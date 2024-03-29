using Newtonsoft.Json;

namespace DCR;
public class NetworkSerializer
{
    public string Serialize(NetworkNode node)
    {
        return JsonConvert.SerializeObject(node, Formatting.Indented);
    }

    public string Serialize(List<NetworkNode> nodes)
    {
        return JsonConvert.SerializeObject(nodes, Formatting.Indented);
    }

    public string Serialize(ConnectRequest connectNode)
    {
        return JsonConvert.SerializeObject(connectNode, Formatting.Indented);
    }

    public string Serialize(ShareBlockRequest shareBlock)
    {
        return JsonConvert.SerializeObject(shareBlock, Formatting.Indented);
    }

    public List<NetworkNode> Deserialize(string json)
    {
        return JsonConvert.DeserializeObject<List<NetworkNode>>(json)!;
    }
}