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
    }
}
