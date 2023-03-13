namespace Models
{
    public class Graph
    {
        public List<Activity> Activities { get; set; }
        public List<Relation> Relations { get; set; }
        public bool Executing { get; set; }

        public Graph(List<Activity> activities, List<Relation> relations, bool executing)
        {
            Activities = activities;
            Relations = relations;
            Executing = executing;
        }

        public Graph()
        {
            Activities = new List<Activity>();
            Relations = new List<Relation>();
            Executing = false;
        }

        public void Execute() { //TODO: Maybe not needed
            Executing = true;
        }
    }
}
