namespace DCR;

public enum Action
{
    Create,
    Update
}
public class Transaction
{
    public string Actor {get; init;}
    public Action Action {get; init;}
    public string Graph {get; init;}
    public Transaction(string actor, Action action, string graph) 
    {
        Actor = actor;
        Action = action;
        Graph = graph;
    }
}