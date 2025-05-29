namespace StudentCanvasApp.Models
{
    public class StudentOverview
    {
        public int StudentID { get; set; }
        public string FullName { get; set; }
        public string ClassNames { get; set; } // joined class names
    }
}
