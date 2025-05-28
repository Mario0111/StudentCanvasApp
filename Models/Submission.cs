using System;

namespace StudentCanvasApp.Models
{
    public class Submission
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string FilePath { get; set; }
        public string SubmissionText { get; set; }
        public int? Grade { get; set; }
    }
}
