using LogBook_API.Domain.Entities;
using LogBook_API.Domain.Repositories;
using LogBook_API.Persistence.RequestParameters;
using Microsoft.EntityFrameworkCore;

namespace LogBook_API.Persistence.Repositories
{
    internal sealed class LogbookEntryRepository : ILogbookEntryRepository
    {
        private readonly RepositoryDbContext _repositoryDbContext;
        public LogbookEntryRepository(RepositoryDbContext repositoryDbContext)
        {
            _repositoryDbContext = repositoryDbContext;
        }
        public async Task<IEnumerable<LogbookEntry>> GetAllAsync(CancellationToken cancellationToken = default) =>
            await _repositoryDbContext.LogbookEntries.ToListAsync(cancellationToken);

        public async Task<IEnumerable<LogbookEntry>> GetAllUserEntriesAsync(string userId, CancellationToken cancellationToken = default) =>
            await _repositoryDbContext.LogbookEntries.Where(l=> l.UserId == userId).ToListAsync(cancellationToken);

        public async Task<IList<LogbookEntry>> GetAllEntriesForCSVDownload(string userId, DateFilterParameter dateFilterParameter, CancellationToken cancellationToken=default)
        {
            var startDateAsDateTime = dateFilterParameter.StartDate.ToDateTime(TimeOnly.MinValue);
            var endDateAsDateTime = dateFilterParameter.EndDate.ToDateTime(TimeOnly.MaxValue);
            var entries =  await _repositoryDbContext.LogbookEntries
                                    .Where(
                                        l => l.UserId == userId && 
                                        (l.EntryDate >= startDateAsDateTime && l.EntryDate <= endDateAsDateTime))
                                    .ToListAsync();
            return entries;
        }

        public async Task<LogbookEntry> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default) =>
            await _repositoryDbContext.LogbookEntries.FirstOrDefaultAsync(l => l.EntryDate.Date.Equals(date.Date), cancellationToken);
        public async Task<IEnumerable<LogbookEntry>> GetEntriesByDateRangeAsync(string userId, IEnumerable<DateTime?> entryDates)
        {
            var validDates = entryDates.Where(date => date.HasValue).Select(date => date.Value.Date).ToList();

            return await _repositoryDbContext.LogbookEntries.Where(e => e.UserId == userId && validDates.Contains(e.EntryDate.Date)).ToListAsync();
        }

        public async Task<LogbookEntry> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            await _repositoryDbContext.LogbookEntries.FirstOrDefaultAsync(l => l.Id.Equals(id), cancellationToken);

        public async Task<IEnumerable<LogbookEntry>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default) =>
            await _repositoryDbContext.LogbookEntries.Where(l=> ids.Contains(l.Id)).ToListAsync();

        public async Task<LogbookEntry> GetEntryByDateAsync(string userId, DateTime date, CancellationToken cancellationToken = default) =>
            await _repositoryDbContext.LogbookEntries.Where(e=> e.UserId == userId).FirstOrDefaultAsync(l=> l.EntryDate.Date.Equals(date.Date),cancellationToken);
        

        public void Insert(LogbookEntry logbookEntry) =>
            _repositoryDbContext.LogbookEntries.Add(logbookEntry);

        public void InsertBatch(IEnumerable<LogbookEntry> logbookEntries) =>
             _repositoryDbContext.LogbookEntries.AddRange(logbookEntries);
        public void UpdateBatch(IEnumerable<LogbookEntry> logbookEntries) =>
             _repositoryDbContext.LogbookEntries.UpdateRange(logbookEntries);

        public void Remove(LogbookEntry logbookEntry) =>
            _repositoryDbContext.LogbookEntries.Remove(logbookEntry);

        public void RemoveBatch(IEnumerable<LogbookEntry> logbookEntries) =>
            _repositoryDbContext.LogbookEntries.RemoveRange(logbookEntries);

        public void Update(LogbookEntry logbookEntry) => 
            _repositoryDbContext.LogbookEntries.Update(logbookEntry);

       
    }

}
