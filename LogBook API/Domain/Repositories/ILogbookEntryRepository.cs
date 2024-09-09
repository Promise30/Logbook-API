using LogBook_API.Domain.Entities;
using LogBook_API.Persistence.RequestParameters;

namespace LogBook_API.Domain.Repositories
{
    public interface ILogbookEntryRepository
    {
        Task<IEnumerable<LogbookEntry>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<LogbookEntry>> GetAllUserEntriesAsync(string userId, CancellationToken cancellationToken = default);
        Task<IList<LogbookEntry>> GetAllEntriesForCSVDownload(string userId, DateFilterParameter dateFilterParameter, CancellationToken cancellationToken = default);
        Task<LogbookEntry> GetByIdAsync(Guid id, CancellationToken cancellationToken=default);
        Task<IEnumerable<LogbookEntry>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken=default);
        Task<LogbookEntry> GetByDateAsync(DateTime date, CancellationToken cancellationToken=default);
        Task<LogbookEntry> GetEntryByDateAsync(string userId,DateTime date, CancellationToken cancellationToken = default);
        Task<IEnumerable<LogbookEntry>> GetEntriesByDateRangeAsync(string userId, IEnumerable<DateTime?> entryDates);
        void Insert(LogbookEntry logbookEntry);
        void InsertBatch(IEnumerable<LogbookEntry> logbookEntries);
        void Update(LogbookEntry logbookEntry);
        void UpdateBatch(IEnumerable<LogbookEntry> logbookEntries);
        void Remove(LogbookEntry logbookEntry);
        void RemoveBatch(IEnumerable<LogbookEntry> logbookEntries);
    }
}
