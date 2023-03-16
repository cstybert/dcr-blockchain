using Models;

namespace DCR;

public enum Action
{
    Create,
    Update
}
public class Transaction
{
    public string Actor { get; init; }
    public Action Action { get; init; }
    public Graph Graph { get; init; }
    public Transaction(string actor, Action action, Graph graph)
    {
        Actor = actor;
        Action = action;
        Graph = graph;
    }
}