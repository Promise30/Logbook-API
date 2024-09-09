using System.ComponentModel.DataAnnotations;

namespace LogBook_API.Contracts
{
    public class LogbookEntryForCreationDto
    {
        [Required(ErrorMessage = "This field is required")]
        public string Activity {  get; set; } = string.Empty;
        [MaxLength(250, ErrorMessage = "This field cannot be more than 250 characters.")]
        public string? Description { get; set; } 
        public DateTime? EntryDate { get; set; } 
    }
}
