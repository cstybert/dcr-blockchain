using Models;

namespace Business
{
    public class GraphExecutor
    {
        private GraphCreator _graphCreator;

        public GraphExecutor() {
            _graphCreator = new GraphCreator();
        }

        public Graph Execute(Graph graph, string executingActivity) // TODO: Create Action/Event data model to incapsulate triggered actions/events
        {
            var source = _graphCreator.GetActivity(executingActivity, graph.Activities);
            source.Executed = true;
            source.Pending = false;

            var relations = graph.Relations.Where(e => e.Source == source.Title);
            foreach (Relation rel in relations) {
                var target = _graphCreator.GetActivity(rel.Target, graph.Activities);
                if (rel.Type == RelationType.EXCLUSION) {
                    target.Included = false;
                } else if (rel.Type == RelationType.INCLUSION) {
                    target.Included = true;
                } else if (rel.Type == RelationType.RESPONSE) {
                    target.Pending = true;
                }
            }

            _graphCreator.UpdateEnabled(graph.Activities, graph.Relations);
            return graph;
        }
    }
}
