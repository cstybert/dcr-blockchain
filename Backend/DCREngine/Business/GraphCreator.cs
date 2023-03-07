using Models;

namespace Business
{
    public class GraphCreator
    {
        public GraphCreator()
        {
	    }

        public Graph create(List<Activity> activities, List<Relation> relations)
        {
            return new Graph(activities, relations, false);
        }

        public Graph create(String input)
        {
            return parseInput(input);
        }

        // TODO: Parse input (single line and multiple) to Graph instance
        private Graph parseInput(String input)
        {
            return new Graph();
        }
    }
}
