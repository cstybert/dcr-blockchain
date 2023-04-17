using Models;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace DCR;

public enum Action
{
    Create,
    Update
}
public class Transaction
{
    public string Id { get ; init; }
    public string Actor { get; init; }
    public Action Action { get; init; }
    public string EntityTitle { get; init; }
    public Graph Graph { get; init; }
    public Transaction(string actor, Action action, string entityTitle, Graph graph)
    {
        Actor = actor;
        Action = action;
        EntityTitle = entityTitle;
        Graph = graph;
        Id = Guid.NewGuid().ToString();
    }

    private string CreateHash() {
        string jsonAction = JsonConvert.SerializeObject(Action);
        string jsonGraph = JsonConvert.SerializeObject(Graph);
        string inputstring = $"{Actor}{Action}{EntityTitle}{jsonAction}{jsonGraph}";

        byte[] inputbytes = Encoding.ASCII.GetBytes(inputstring);
        byte[] hash = SHA256.HashData(inputbytes);
        return Convert.ToBase64String(hash);
    }
}