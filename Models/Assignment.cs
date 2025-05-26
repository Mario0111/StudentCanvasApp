using System;

namespace StudentCanvasApp.Models
{
    public class Assignment
    {
        public int AssignmentID { get; set; }
        public int? ClassID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
    }
}
