public class ExecuteActivityRequest
{
    public string Actor {get; init;}
    public string ExecutingActivity {get; init;}

    public ExecuteActivityRequest(string actor, string executingActivity) 
    {
        Actor = actor;
        ExecutingActivity = executingActivity;
    }
}