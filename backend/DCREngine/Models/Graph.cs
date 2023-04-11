using Newtonsoft.Json;
namespace Models
{
    public class Graph
    {
        public string Id { get; set; }
        public List<Activity> Activities { get; set; }
        public List<Relation> Relations { get; set; }

        public Graph()
        {
            Id = Guid.NewGuid().ToString();
            Activities = new List<Activity>();
            Relations = new List<Relation>();
        }

        public Graph(List<Activity> activities, List<Relation> relations)
        {
            Id = Guid.NewGuid().ToString();
            Activities = activities;
            Relations = relations;
        }

        [JsonConstructor]
        private Graph(string id, List<Activity> activities, List<Relation> relations)
        {
            Id = id;
            Activities = activities;
            Relations = relations;
        }

        public bool Accepting {
            get {
                return Activities.All(e => !(e.Included && e.Pending));
            }
        }

        public bool EqualsGraph(Graph otherGraph)
        {
            if (otherGraph is null) return false;
            return  (Id == otherGraph.Id) &&
                    (Relations.SequenceEqual(otherGraph.Relations)) &&
                    (!Activities.SequenceEqual(otherGraph.Activities)) &&
                    (Accepting == otherGraph.Accepting);
        }
    }
}
