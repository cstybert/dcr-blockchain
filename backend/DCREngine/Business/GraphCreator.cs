using Models;

namespace Business
{
    public class GraphCreator
    {
        public Graph Create(List<Activity> activities, List<Relation> relations)
        {
            UpdateEnabled(activities, relations);
            return new Graph(activities, relations);
        }

        public void UpdateEnabled(List<Activity> activities, List<Relation> relations) {
            foreach (Relation rel in relations) {
                var source = GetActivity(rel.Source, activities);
                var target = GetActivity(rel.Target, activities);

                if (!target.Included) {
                    target.Enabled = false;
                } else if (rel.Type == RelationType.CONDITION) {
                    if (source.Included && !source.Executed) {
                        target.Enabled = false;
                    } else {
                        target.Enabled = true;
                    }
                }
            }
        }

        public Activity GetActivity(string title, List<Activity> activities)
        {
            return activities.Single(a => a.Title == title);
        }
    }
}
