using LogBook_API.Contracts.Auth;
using LogBook_API.Domain.Entities;

namespace LogBook_API.Contracts.MappingExtensions
{
    public static class MappingExtensions
    {
        public static UserResponseDto ToUserResponseDto(this User user)
        {
            if(user != null)
            {
                return new UserResponseDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    NormalizedUserName = user.NormalizedUserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    NormalizedEmail = user.NormalizedEmail,
                    PhoneCountryCode = user.PhoneCountryCode,
                    PhoneNumber = user.PhoneNumber,
                    DateCreated = user.CreatedDate,
                    DateModified = user.LastUpdatedDate
                };
            }
            return null;
        }
        public static LogbookEntryDto ToDto(this LogbookEntry logbookEntry)
        {
            if(logbookEntry != null)
            {
                return new LogbookEntryDto
                {
                    Id = logbookEntry.Id,
                    Activity = logbookEntry.Activity,
                    Description = logbookEntry.Description,
                    EntryDate = logbookEntry.EntryDate,
                    LastUpdatedDate = logbookEntry.LastUpdatedDate, 
                    
                };
            }
            return null;
        }
        
        public static LogbookEntry ToLogBookModel(this LogbookEntryDto logbookEntryDto) 
        {
            if (logbookEntryDto != null)
            {
                return new LogbookEntry
                {
                    Id = logbookEntryDto.Id,
                    Activity = logbookEntryDto.Activity,
                    Description = logbookEntryDto.Description,
                    LastUpdatedDate = logbookEntryDto.LastUpdatedDate,
                };
            }
            return null;
        }
    }
}
