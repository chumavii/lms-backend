namespace LmsApi.Models
{
    public class Lesson
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required string ContentUrl { get; set; }
        public Guid ModuleId { get; set; }
        public required Module Module { get; set; }
    }
}
