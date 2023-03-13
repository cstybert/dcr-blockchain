using Models;

namespace Business
{
    public class GraphExecutor
    {
        public Graph Execute(Graph graph, string activityTitle) // TODO: Create Action/Event data model to incapsulate triggered actions/events
        {
            var activity = GetActivity(activityTitle, graph.Activities);

            if (!activity.Enabled) {
                return graph;
            }

            activity.Executed = true;
            activity.Pending = false;

            var relations = graph.Relations.Where(e => e.Target.Title == activity.Title);
            foreach (Relation rel in relations) {
                if (rel.Type == RelationType.EXCLUSION) {
                    rel.Target.Included = false;
                } else if (rel.Type == RelationType.INCLUSION) {
                    rel.Target.Included = true;
                } else if (rel.Type == RelationType.RESPONSE) {
                    rel.Target.Pending = true;
                }
            }

            return graph; // TODO: Execute action/event on current graph state, return resuting graph state
        }

        private Activity GetActivity(string title, List<Activity> activities)
        {
            return activities.Single(e => e.Title == title);
        }
    }
}
