using Newtonsoft.Json;
namespace Models
{
    public class Graph
    {
        public string Id { get; set; }
        public List<Activity> Activities { get; set; }
        public List<Relation> Relations { get; set; }

        public Graph() : this(Guid.NewGuid().ToString(), new List<Activity>(), new List<Relation>())
        {
        }

        public Graph(List<Activity> activities, List<Relation> relations) : this(Guid.NewGuid().ToString(), activities, relations)
        {
        }

        [JsonConstructor]
        private Graph(string id, List<Activity> activities, List<Relation> relations)
        {
            Id = id;
            Activities = activities;
            Relations = relations;
            UpdateActivitiesEnabled();
        }

        public bool Accepting
        {
            get
            {
                return Activities.All(e => !(e.Included && e.Pending));
            }
        }

        private Activity GetActivity(string title)
        {
            return Activities.Single(a => a.Title == title);
        }

        // Update the enabled status of all activities in graph
        private void UpdateActivitiesEnabled()
        {
            foreach (Activity activity in Activities)
            {
                // If activity is not included, disable
                if (!activity.Included)
                {
                    activity.Enabled = false;
                }
                else
                {
                    // If not all included conditions are met, disable
                    var conditions = Relations.Where(r => (r.Type == RelationType.CONDITION) && (GetActivity(r.Source).Included) && (r.Target == activity.Title));
                    activity.Enabled = conditions.All(r => GetActivity(r.Source).Executed);
                }
            }
        }

        // Execute activity in graph
        public void Execute(string activity)
        {
            var source = GetActivity(activity);
            source.Executed = true;
            source.Pending = false;

            // Get relations where input activity is source
            var relations = Relations.Where(e => e.Source == source.Title);
            foreach (Relation rel in relations)
            {
                var target = GetActivity(rel.Target);
                if (rel.Type == RelationType.EXCLUSION)
                {
                    target.Included = false;
                }
                else if (rel.Type == RelationType.INCLUSION)
                {
                    target.Included = true;
                }
                else if (rel.Type == RelationType.RESPONSE)
                {
                    target.Pending = true;
                }
            }

            UpdateActivitiesEnabled();
        }

        public bool EqualsGraph(Graph otherGraph)
        {
            if (otherGraph is null) return false;
            return (Id == otherGraph.Id) &&
                    (Relations.SequenceEqual(otherGraph.Relations)) &&
                    (Activities.SequenceEqual(otherGraph.Activities)) &&
                    (Accepting == otherGraph.Accepting);
        }
    }
}