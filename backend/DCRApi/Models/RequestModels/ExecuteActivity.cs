using Models;

public class ExecuteActivity
{
    public string Actor {get; init;}
    public string ExecutingActivity {get; init;}

    public ExecuteActivity(string actor, string executingActivity) 
    {
        Actor = actor;
        ExecutingActivity = executingActivity;
    }
}