namespace LogBook_API.Domain.Entities
{
    public class LogbookEntry
    {
        public Guid Id { get; set; }
        public DateTime EntryDate { get; set; }
        public string Activity { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
    }
}
