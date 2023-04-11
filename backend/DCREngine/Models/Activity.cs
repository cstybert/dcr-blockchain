namespace Models
{
    public class Activity
    {
        public string Title { get; set; }
        public bool Pending { get; set; }
        public bool Included { get; set; }
        public bool Executed { get; set; }
        public bool Enabled { get; set; }

        public Activity() {
            Title = "";
        }

        public Activity(string title)
        {
            Title = title;
            Pending = false;
            Included = true;
            Executed = false;
            Enabled = true;
        }


        public Activity(string title, bool pending) {
            Title = title;
            Pending = pending;
            Included = true;
            Executed = false;
            Enabled = true;
        }

        public Activity(string title, bool pending, bool included, bool executed)
        {
            Title = title;
            Pending = pending;
            Included = included;
            Executed = executed;
            Enabled = true;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (Activity)obj;
            return (Title == other.Title) && (Pending == other.Pending) && (Included == other.Included) && (Executed == other.Executed) && (Enabled == other.Enabled);
        }
    }
}
