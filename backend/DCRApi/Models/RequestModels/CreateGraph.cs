using Models;

public class CreateGraph
{
    public string Actor {get; init;}
    public Graph Graph {get; init;}

    public CreateGraph(string actor, Graph graph) 
    {
        Actor = actor;
        Graph = graph;
    }
}