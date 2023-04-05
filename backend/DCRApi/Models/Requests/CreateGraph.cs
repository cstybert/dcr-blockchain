using Models;

public class CreateGraph
{
    public string Actor {get; init;}
    public List<Activity> Activities {get; init;}
    public List<Relation> Relations {get; init;}

    public CreateGraph(string actor, List<Activity> activities, List<Relation> relations) 
    {
        Actor = actor;
        Activities = activities;
        Relations = relations;
    }
}