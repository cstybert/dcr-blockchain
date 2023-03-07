namespace Models
{
    public class Relation
    {
        public RelationType Type { get; set; }
        public Activity Source { get; set; }
        public Activity Target { get; set; }

        public Relation(RelationType type, Activity source, Activity target)
        {
            Type = type;
            Source = source;
            Target = target;
        }

        public override string ToString()
        {
            var typ = RelationTypeMethods.ParseRelationTypeToString(Type).Replace("\\", "");
            var src = Source != null ? Source.Title : "<none>";
            var trg = Target != null ? Target.Title : "<none>";
            return $"{src}{typ}{trg}";
        }
    }
}
