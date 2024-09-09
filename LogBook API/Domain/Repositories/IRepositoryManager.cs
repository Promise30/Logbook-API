namespace LogBook_API.Domain.Repositories
{
    public interface IRepositoryManager
    {
        ILogbookEntryRepository LogbookEntryRepository { get; }
        Task Save();
    }
}
