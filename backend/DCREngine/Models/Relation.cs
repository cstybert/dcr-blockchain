namespace Models
{
    public class Relation
    {
        public RelationType Type { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }

        public Relation() {
            Source = "";
            Target = "";
        }

        public Relation(RelationType type, string source, string target)
        {
            Type = type;
            Source = source;
            Target = target;
        }

        public Relation(RelationType type, Activity source, Activity target)
        {
            Type = type;
            Source = source.Title;
            Target = target.Title;
        }

        public override string ToString()
        {
            var typ = RelationTypeMethods.ParseRelationTypeToString(Type).Replace("\\", "");
            var src = Source != null ? Source : "<none>";
            var trgt = Target != null ? Target : "<none>";
            return $"{src}{typ}{trgt}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Relation other = (Relation)obj;
            return (Type == other.Type) && (Source == other.Source) && (Target == other.Target);
        }
    }
}
