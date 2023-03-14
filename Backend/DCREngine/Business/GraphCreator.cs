using Models;
using System.Text.RegularExpressions;

namespace Business
{
    public class GraphCreator
    {
        private Regex _actRegex;
	    private Regex _relRegex;
        
        public GraphCreator()
        {
            // Regex (?<!-->\*)"([a-zA-Z0-9_]+!?)"(?!-->*), e.g. "ActivityA" (not followed or preceded by relation)
            var actPattern = "(\"[a-zA-Z0-9_]+\"[!]?)";
            _actRegex = new Regex("(?<!"+ GetRelationTypesString() +")" + actPattern + "(?!" + GetRelationTypesString() +")", RegexOptions.Compiled);

            // Regex matches e.g. "ActivityA"<relationType>"ActivityB"
            var relPattern = actPattern +"("+ GetRelationTypesString() +")"+ actPattern;
            _relRegex = new Regex(relPattern, RegexOptions.Compiled);
	    }

        public Graph Create(List<Activity> activities, List<Relation> relations)
        {
            UpdateEnabled(activities, relations);
            return new Graph(activities, relations);
        }

        public Graph Create(String input)
        {
            return ParseInput(input);
        }

        public void UpdateEnabled(List<Activity> activities, List<Relation> relations) {
            foreach (Relation rel in relations) {
                rel.Source = GetActivity(rel.Source.Title, activities);
                rel.Target = GetActivity(rel.Target.Title, activities);

                if (!rel.Target.Included) {
                    rel.Target.Enabled = false;
                } else if (rel.Type == RelationType.CONDITION) {
                    if (rel.Source.Included && !rel.Source.Executed) {
                        rel.Target.Enabled = false;
                    } else {
                        rel.Target.Enabled = true;
                    }
                }
            }
        }

        // TODO: Parse input (single line and multiple) to Graph instance
        private Graph ParseInput(string input)
        {
            var activities = ParseActivities(input);
            var relations = ParseRelations(input, activities);

            return new Graph(activities, relations);
        }

        private List<Activity> ParseActivities(string input)
        {
            var activities = new List<Activity>();
            var actMatches = _actRegex.Matches(input);
    
            foreach (Match match in actMatches) {
                var groups = match.Groups;
                var activity = ParseActivity(groups[0].Value);
                activities.Add(activity);
            }
            return activities;
	    }

        private List<Relation> ParseRelations(string input, List<Activity> activities)
        {
            var relations = new List<Relation>();
            var relMatches = _relRegex.Matches(input);

            foreach (Match match in relMatches) {
                var groups = match.Groups;

                // Get existing activities
                var src = GetActivity(groups[1].Value, activities);
                var trgt = GetActivity(groups[3].Value, activities);

                var parsedRelationType = RelationTypeMethods.ParseStringToRelationType(groups[2].Value);
                var relation = new Relation(parsedRelationType, src, trgt);
                
                relations.Add(relation);
            }
            return relations;
        }
        
        private Activity ParseActivity(string captured)
        {
            string charsToRemove = "[!()\"]";     
            string title = Regex.Replace(captured, charsToRemove, String.Empty);
            bool isPending = captured.Contains('!');
            return new Activity(title, isPending);
        }

        private Activity GetActivity(string title, List<Activity> activities)
        {
            return activities.Single(e => e.Title == title.Replace("\"", ""));
        }

        private string GetRelationTypesString() {
            var relationTypeList = Enum.GetValues(typeof(RelationType));

            var relationStrings = new List<string>();
            foreach (RelationType relationType in relationTypeList) {
                var relationString = RelationTypeMethods.ParseRelationTypeToString(relationType);
                relationStrings.Add(relationString);		
            }

            return String.Join("|", relationStrings);
        }
    }
}
