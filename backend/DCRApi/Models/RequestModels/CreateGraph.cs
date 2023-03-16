public class CreateGraph
{
    public string Actor {get; init;}
    public string Graph {get; init;}

    public CreateGraph(string actor, string graph) 
    {
        Actor = actor;
        Graph = graph;
    }
}