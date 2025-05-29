using System.Collections.Generic;

namespace StudentCanvasApp.Models
{
    public class Student
    {
        public int StudentID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName => $"{Name} {LastName}";
        public List<string> Classes { get; set; }
        public string ClassesJoined => string.Join(", ", Classes);


    }
}
