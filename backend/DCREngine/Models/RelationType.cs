namespace Models
{
    public enum RelationType
    {
        CONDITION,
        RESPONSE,
        INCLUSION,
        EXCLUSION
    }

    public static class RelationTypeMethods
    {

        public static RelationType ParseStringToRelationType(this String typeStr)
        {
            switch (typeStr)
            {
                case "-->*":
                    return RelationType.CONDITION;
                case "*-->":
                    return RelationType.RESPONSE;
                case "-->%":
                    return RelationType.EXCLUSION;
                case "-->+":
                    return RelationType.INCLUSION;
                default:
                    throw new NotImplementedException();
            }
        }

        public static string ParseRelationTypeToString(this RelationType type)
        {
            switch (type)
            {
                case RelationType.CONDITION:
                    return @"-->\*";
                case RelationType.RESPONSE:
                    return @"\*-->";
                case RelationType.EXCLUSION:
                    return @"-->\%";
                case RelationType.INCLUSION:
                    return @"-->\+";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
