using Newtonsoft.Json;

namespace DCR;
public class NetworkSerializer
{
    public string Serialize(Node node) {
        return JsonConvert.SerializeObject(node, Formatting.Indented);
    }

    public string Serialize(List<Node> nodes) {
        return JsonConvert.SerializeObject(nodes, Formatting.Indented);
    }

    public string Serialize(ConnectNode connectNode) {
        return JsonConvert.SerializeObject(connectNode, Formatting.Indented);
    }

    public List<Node> Deserialize(string json) {
        return JsonConvert.DeserializeObject<List<Node>>(json)!;
    }
}