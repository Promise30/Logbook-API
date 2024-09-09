using LogBook_API.Contracts;
using LogBook_API.Persistence.RequestParameters;

namespace LogBook_API.Services.Abstractions
{
    public interface ILogbookEntryService
    {
        Task<ApiResponse<IEnumerable<LogbookEntryDto>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<LogbookEntryDto>>> GetAllEntriesAsync(CancellationToken cancellationToken = default);
        
        Task<ApiResponse<LogbookEntryDto>> GetEntryByIdAsync(Guid Id, CancellationToken cancellationToken = default);
        Task<ApiResponse<LogbookEntryDto>> CreateEntryAsync(LogbookEntryForCreationDto logbookEntryForCreationDto, CancellationToken cancellation = default);
        Task<ApiResponse<IEnumerable<LogbookEntryDto>>> CreateEntriesAsync(IEnumerable<LogbookEntryForCreationDto> logbookEntryForCreationDtos, CancellationToken cancellation = default);
        Task<ApiResponse<LogbookEntryDto>> UpdateEntryAsync(Guid id, LogbookEntryForUpdateDto logbookEntryForUpdateDto, CancellationToken cancellationToken = default);
        Task<ApiResponse<object>> DeleteEntryAsync(Guid Id, CancellationToken cancellationToken = default);
        Task<ApiResponse<object>> DeleteEntriesAsync(IEnumerable<Guid> entryIds, CancellationToken cancellationToken = default);
        Task<byte[]> GetEntriesAsCSVFormat(DateFilterParameter dateFilterParameter,CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<LogbookEntryDto>>> UpdateMultipleEntriesAsync(IEnumerable<LogbookEntryForUpdateDto> logbookEntriesToUpdate, CancellationToken cancellationToken = default);
    }
}
