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
            foreach (Activity activity in activities) {
                // If activity is not included, disable
                if (!activity.Included) {
                    activity.Enabled = false;
                } else {
                    // If not all included conditions are met, disable
                    var conditions = relations.Where(r => (r.Type == RelationType.CONDITION) && (GetActivity(r.Source, activities).Included) && (r.Target == activity.Title));
                    activity.Enabled = conditions.All(r => GetActivity(r.Source, activities).Executed);
                }
            }
        }

        public Activity GetActivity(string title, List<Activity> activities)
        {
            return activities.Single(a => a.Title == title);
        }
    }
}
