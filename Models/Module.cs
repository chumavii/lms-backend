namespace LmsApi.Models
{
    public class Module
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required int CourseId { get; set; }
        public required Course Course { get; set; }

        public ICollection<Lesson>? Lessons { get; set; }
    }
}
