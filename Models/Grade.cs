namespace StudentCanvasApp.Models
{
    public class Grade
    {
        public int GradeID { get; set; }
        public int? StudentID { get; set; }
        public int? ClassID { get; set; }
        public float? Score { get; set; }
    }
}
