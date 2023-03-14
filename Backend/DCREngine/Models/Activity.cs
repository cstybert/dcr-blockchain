namespace Models
{
    public class Activity
    {
        public string Title { get; set; }
        public bool Pending { get; set; }
        public bool Included { get; set; }
        public bool Executed { get; set; }
        public List<Relation> Relations { get; set; }

        public Activity(string title, bool pending, bool included, bool executed, List<Relation> relations)
        {
            Title = title;
            Pending = pending;
            Included = included;
            Executed = executed;
            Relations = relations;
        }

        public Activity(string title, bool pending) {
            Title = title;
            Pending = pending;
            Included = true;
            Executed = false;
            Relations = new List<Relation>();
        }

        public Activity(string title)
        {
            Title = title;
            Pending = false;
            Included = true;
            Executed = false;
            Relations = new List<Relation>();
        }

        //TODO: Verify implementation
        public bool Enabled 
        {
            get {
                if (!this.Included) {
                    return false;
                }
                foreach (Relation rel in Relations) {
                    if (rel.Source.Title != Title && rel.Source.Included && !rel.Source.Executed) {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
