namespace Models
{
    public class Graph
    {
        public string id { get; } = Guid.NewGuid().ToString();  
        public List<Activity> Activities { get; set; }
        public List<Relation> Relations { get; set; }

        public Graph()
        {
            Activities = new List<Activity>();
            Relations = new List<Relation>();
        }

        public Graph(List<Activity> activities, List<Relation> relations)
        {
            Activities = activities;
            Relations = relations;
        }

        public bool Accepting {
            get {
                return Activities.All(e => !(e.Included && e.Pending));
            }
        }
    }
}
