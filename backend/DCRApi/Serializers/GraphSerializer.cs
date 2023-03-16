using Newtonsoft.Json;
using Models;

namespace DCR;
public class GraphSerializer
{
    public string Serialize(Graph graph) {
        return JsonConvert.SerializeObject(graph, Formatting.Indented);
    }

    public Graph Deserialize(string json) {
        return JsonConvert.DeserializeObject<Graph>(json)!;
    }
}