using Models;

public class ExecuteActivity
{
    public string Actor {get; init;}
    public Graph Graph {get; init;}
    public string ExecutingActivity {get; init;}

    public ExecuteActivity(string actor, Graph graph, string executingActivity) 
    {
        Actor = actor;
        Graph = graph;
        ExecutingActivity = executingActivity;
    }
}