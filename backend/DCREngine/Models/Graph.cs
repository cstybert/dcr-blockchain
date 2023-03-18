using Newtonsoft.Json;
namespace Models
{
    public class Graph
    {
        public string ID { get; set; }
        public List<Activity> Activities { get; set; }
        public List<Relation> Relations { get; set; }

        public Graph()
        {
            ID = Guid.NewGuid().ToString();
            Activities = new List<Activity>();
            Relations = new List<Relation>();
        }

        public Graph(List<Activity> activities, List<Relation> relations)
        {
            ID = Guid.NewGuid().ToString();
            Activities = activities;
            Relations = relations;
        }

        [JsonConstructor]
        private Graph(string id, List<Activity> activities, List<Relation> relations)
        {
            ID = id;
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
