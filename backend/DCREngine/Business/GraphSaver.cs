using Models;
using Newtonsoft.Json;

namespace Business;

public class GraphSaver
{
    private string _graphPath;

    public GraphSaver(string graphPath)
    {
        _graphPath = graphPath;
    }

    public void SaveGraph(Graph graph)
    {
        var jsonString = JsonConvert.SerializeObject(graph, Formatting.Indented);
        File.WriteAllText(_graphPath, jsonString);
    }

    public Graph LoadGraph()
    {
        var reader = new StreamReader(_graphPath);
        var json = reader.ReadToEnd();
        reader.Close();
        var graph = JsonConvert.DeserializeObject<Graph>(json)!;
        return graph;
    }
}
