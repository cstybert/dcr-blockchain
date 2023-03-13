namespace Models
{
    public class Graph
    {
        public List<Activity> Activities { get; set; }
        public List<Relation> Relations { get; set; }

        public Graph(List<Activity> activities, List<Relation> relations)
        {
            Activities = activities;
            Relations = relations;
        }

        public Graph()
        {
            Activities = new List<Activity>();
            Relations = new List<Relation>();
        }
    }
}
