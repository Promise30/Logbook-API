namespace LogBook_API.Contracts
{
    public class LogbookEntryDto
    {
        public Guid Id { get; set; }
        public string Activity { get; set; } = string.Empty;
        public string? Description { get; set; } 
        public DateTime? EntryDate { get; set; } 
        public DateTime LastUpdatedDate { get; set; }
    }
}
