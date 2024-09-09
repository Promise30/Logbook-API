using LogBook_API.Domain.Repositories;

namespace LogBook_API.Persistence.Repositories
{
    public sealed class RepositoryManager : IRepositoryManager
    {
        private readonly RepositoryDbContext _repositoryDbContext;
        private readonly Lazy<ILogbookEntryRepository> _logbookEntryRepository;
        public RepositoryManager(RepositoryDbContext repositoryDbContext)
        {
            _repositoryDbContext = repositoryDbContext;
            _logbookEntryRepository = new Lazy<ILogbookEntryRepository>(() => new LogbookEntryRepository(repositoryDbContext));
            
        }
        public ILogbookEntryRepository LogbookEntryRepository => _logbookEntryRepository.Value;

        public async Task Save() => await _repositoryDbContext.SaveChangesAsync();
        
    }
}
