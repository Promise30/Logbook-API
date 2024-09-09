using LogBook_API.Contracts;
using LogBook_API.Contracts.Auth;
using LogBook_API.Contracts.MappingExtensions;
using LogBook_API.Domain.Entities;
using LogBook_API.Domain.Repositories;
using LogBook_API.Persistence.RequestParameters;
using LogBook_API.Services.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace LogBook_API.Services
{
    internal sealed class LogbookEntryService : ILogbookEntryService
    {
        private readonly ILogger<LogbookEntryService> _logger;
        private readonly IRepositoryManager _repositoryManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICSVService _csvService;
        private readonly IMemoryCache _memoryCache;
        public LogbookEntryService(IRepositoryManager repositoryManager, IHttpContextAccessor httpContextAccessor, ILogger<LogbookEntryService> logger, ICSVService csvService, IMemoryCache memoryCache)
        {
            _repositoryManager = repositoryManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _csvService = csvService;
            _memoryCache = memoryCache;
        }
        public async Task<ApiResponse<LogbookEntryDto>> CreateEntryAsync(LogbookEntryForCreationDto logbookEntryForCreationDto, CancellationToken cancellation = default)
        {
            try
            {
                DateTime entryDate = logbookEntryForCreationDto.EntryDate ?? DateTime.Today;
                var userId = GetCurrentUserId();
                if (entryDate.Date < DateTime.Today)
                {
                    return ApiResponse<LogbookEntryDto>.Failure(400, "Bad Request. You cannot add a new entry for past dates.");
                }
                if (entryDate.Date == DateTime.Today)
                {
                    logbookEntryForCreationDto.EntryDate = DateTime.Today;
                }

                var existingEntry = await _repositoryManager.LogbookEntryRepository.GetEntryByDateAsync(userId, entryDate);

                if (existingEntry != null)
                {
                    _logger.Log(LogLevel.Information, "Existing entry retrieved from database: {existingEntry}", existingEntry.CreatedDate);
                    return ApiResponse<LogbookEntryDto>.Failure(400, "An entry for this date already exists.");
                }
                var entryToCreate = new LogbookEntry
                {
                    Activity = logbookEntryForCreationDto.Activity,
                    Description = logbookEntryForCreationDto.Description,
                    EntryDate = entryDate,
                    UserId = userId
                };
                _repositoryManager.LogbookEntryRepository.Insert(entryToCreate);
                await _repositoryManager.Save();

                // Invalidate cache
                InvalidateUserCache(userId);
                _logger.Log(LogLevel.Information, $"Newly created entry at {entryToCreate.CreatedDate} with Id '{entryToCreate.Id}.");
                var entryToReturn = entryToCreate.ToDto();
                return ApiResponse<LogbookEntryDto>.Success(201, entryToReturn, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while creating a new logbook entry: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<LogbookEntryDto>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }
        public async Task<ApiResponse<object>> DeleteEntryAsync(Guid Id, CancellationToken cancellationToken = default)
        {
            try
            {
                var entry = await _repositoryManager.LogbookEntryRepository.GetByIdAsync(Id, cancellationToken);
                if (entry is null)
                {
                    _logger.Log(LogLevel.Information, $"Failed to retrieved entry to be deleted with Id '{Id}' from the database.");
                    return ApiResponse<object>.Failure(400, "Invalid logbook entry Id");
                }
                // check if the Id of the entry creator matches that of the user trying to delete the entry
                if (entry.UserId != GetCurrentUserId())
                    return ApiResponse<object>.Failure(403, "You do not have the access to delete this entry");
                _repositoryManager.LogbookEntryRepository.Remove(entry);
                await _repositoryManager.Save();
                // Invalidate the cache
                var userId = GetCurrentUserId();
                InvalidateUserCache(userId);
                InvalidateEntryCache(GetCurrentUserId(), Id);

                _logger.Log(LogLevel.Information, $"Entry with id '{Id}' deleted successfully from the database.");
                return ApiResponse<object>.Success(204, null);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while deleting a logbook entry: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);

                return ApiResponse<object>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }
        public async Task<ApiResponse<IEnumerable<LogbookEntryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var entriesFromDb = await _repositoryManager.LogbookEntryRepository.GetAllAsync(cancellationToken);
                _logger.Log(LogLevel.Information, $"Total entries retrieved from the database: {entriesFromDb.Count()}");
                var entriesToReturn = entriesFromDb.Select(e => e.ToDto()).ToList();
                return ApiResponse<IEnumerable<LogbookEntryDto>>.Success(200, entriesToReturn, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while retrieving logbook entries from the database: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<IEnumerable<LogbookEntryDto>>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }
        public async Task<ApiResponse<IEnumerable<LogbookEntryDto>>> GetAllEntriesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var cacheKey = $"LogbookEntries_{currentUserId}";
                if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<LogbookEntryDto> entriesToReturn))
                {
                    var entriesFromDb = await _repositoryManager.LogbookEntryRepository.GetAllUserEntriesAsync(currentUserId, cancellationToken);
                    _logger.Log(LogLevel.Information, "Total entries retrieved from the database: {Count}", entriesFromDb.Count());
                    entriesToReturn = entriesFromDb.Select(e => e.ToDto()).ToList();

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                                                .SetSlidingExpiration(TimeSpan.FromMinutes(2))
                                                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

                    _memoryCache.Set(cacheKey, entriesToReturn, cacheEntryOptions);
                }
                else
                {
                    _logger.Log(LogLevel.Information, "Retrieved {Count} entries from cache for user {UserId}", entriesToReturn.Count(), currentUserId);
                }
                return ApiResponse<IEnumerable<LogbookEntryDto>>.Success(200, entriesToReturn, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while retrieving user logbook entries from the database: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<IEnumerable<LogbookEntryDto>>.Failure(500, "An error occurred. Request unsuccessful.");
            }

        }

        public async Task<ApiResponse<LogbookEntryDto>> GetEntryByIdAsync(Guid Id, CancellationToken cancellationToken = default)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var cacheKey = $"LogbookEntry_{currentUserId}_{Id}";
                if (!_memoryCache.TryGetValue(cacheKey, out LogbookEntryDto entryDto))
                {
                    var entryFromDb = await _repositoryManager.LogbookEntryRepository.GetByIdAsync(Id, cancellationToken);
                    if (entryFromDb is null)
                    {
                        _logger.Log(LogLevel.Information, "Failed to retrieved entry with Id '{Id}' from the database.", Id);
                        return ApiResponse<LogbookEntryDto>.Failure(400, "Invalid logbook entry Id");
                    }
                    entryDto = entryFromDb.ToDto();
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                                .SetSlidingExpiration(TimeSpan.FromMinutes(2))
                                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                    _memoryCache.Set(cacheKey, entryDto, cacheEntryOptions);
                }

                return ApiResponse<LogbookEntryDto>.Success(200, entryDto, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while retrieving logbook entry from the database: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<LogbookEntryDto>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }
        public async Task<byte[]> GetEntriesAsCSVFormat(DateFilterParameter dateFilterParameter, CancellationToken cancellationToken = default)
        {
            try
            {
                var currentUser = GetCurrentUserId();
                var entries = await _repositoryManager.LogbookEntryRepository.GetAllEntriesForCSVDownload(currentUser, dateFilterParameter, cancellationToken);
                var entriesDto = entries.Select(e => e.ToDto()).ToList();
                // Generate CSV in memory and return as a byte array
                return _csvService.WriteCSV(entriesDto);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while processing the request: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return Array.Empty<byte>();
            }
        }

        public async Task<ApiResponse<LogbookEntryDto>> UpdateEntryAsync(Guid id, LogbookEntryForUpdateDto logbookEntryForUpdateDto, CancellationToken cancellationToken = default)
        {
            try
            {
                var existingEntry = await _repositoryManager.LogbookEntryRepository.GetByIdAsync(id, cancellationToken);
                if (existingEntry is null)
                {
                    _logger.Log(LogLevel.Information, "Failed to retrieved entry to be updated with Id '{id}' from the database.", id);
                    return ApiResponse<LogbookEntryDto>.Failure(404, "Logbook entry does not exist. Request unsuccessful");
                }
                // check if the Id of the entry creator matches that of the user trying to update the entry
                if (existingEntry.UserId != GetCurrentUserId())
                    return ApiResponse<LogbookEntryDto>.Failure(403, "You do not have the access to update this entry");
                // Perform the update
                existingEntry.EntryDate = logbookEntryForUpdateDto.EntryDate;
                existingEntry.Activity = logbookEntryForUpdateDto.Activity;
                existingEntry.Description = logbookEntryForUpdateDto.Description;
                existingEntry.LastUpdatedDate = DateTime.Now;

                _repositoryManager.LogbookEntryRepository.Update(existingEntry);
                await _repositoryManager.Save();
                //Invalidate the cache
                var userId = GetCurrentUserId();
                InvalidateUserCache(userId);
                InvalidateEntryCache(userId, id);
                _logger.Log(LogLevel.Information, "Newly updated entry: {existingEntry}", existingEntry);

                var entryToReturn = existingEntry.ToDto();
                return ApiResponse<LogbookEntryDto>.Success(200, entryToReturn, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while updating logbook entry: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<LogbookEntryDto>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }
        public async Task<ApiResponse<IEnumerable<LogbookEntryDto>>> CreateEntriesAsync(IEnumerable<LogbookEntryForCreationDto> logbookEntryForCreationDtos, CancellationToken cancellation = default)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var entryDates = logbookEntryForCreationDtos.Select(d => d.EntryDate).Where(date => date.HasValue).ToList();
                var existingEntries = await _repositoryManager.LogbookEntryRepository.GetEntriesByDateRangeAsync(currentUserId, entryDates);
                var existingEntryDates = existingEntries.Select(e => e.EntryDate.Date).ToHashSet();
                var createdEntries = new List<LogbookEntry>();
                var failedEntries = new List<(LogbookEntryForCreationDto entry, string reason)>();

                foreach (var entryDto in logbookEntryForCreationDtos)
                {
                    var entryDate = entryDto.EntryDate?.Date ?? DateTime.Today;
                    if (entryDate < DateTime.Today)
                    {
                        failedEntries.Add((entryDto, "Cannot add entries for past dates"));
                        continue;
                    }
                    if (entryDate == DateTime.Today)
                    {
                        entryDate = DateTime.Today;
                    }
                    if (existingEntryDates.Contains(entryDate))
                    {
                        failedEntries.Add((entryDto, "An entry for this date already exists"));
                        continue;
                    }
                    var entryToCreate = new LogbookEntry
                    {
                        Activity = entryDto.Activity,
                        Description = entryDto.Description,
                        EntryDate = entryDate,
                        UserId = currentUserId,
                    };
                    createdEntries.Add(entryToCreate);
                }
                if (createdEntries.Any())
                {
                    _repositoryManager.LogbookEntryRepository.InsertBatch(createdEntries);
                    await _repositoryManager.Save();
                    // invalidate cache
                    InvalidateUserCache(currentUserId);
                    _logger.Log(LogLevel.Information, "{Count} new entries created for user {currentUserId}", createdEntries.Count, currentUserId);
                }
                var successfulEntries = createdEntries.Select(e => e.ToDto()).ToList();
                if (!failedEntries.Any())
                {
                    return ApiResponse<IEnumerable<LogbookEntryDto>>.Success(201, successfulEntries, "Request successful");
                }
                else
                {
                    var message = $"{successfulEntries.Count} entries created successfully. {failedEntries.Count} entries failed";
                    _logger.Log(LogLevel.Information, message);
                    var errorDetails = failedEntries.Select(f => $"Entry for {f.entry.EntryDate}: {f.reason}").ToList();
                    return ApiResponse<IEnumerable<LogbookEntryDto>>.PartialSuccess(207, successfulEntries, message, errorDetails);
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while creating new user logbook entries: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<IEnumerable<LogbookEntryDto>>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }
        public async Task<ApiResponse<IEnumerable<LogbookEntryDto>>> UpdateMultipleEntriesAsync(IEnumerable<LogbookEntryForUpdateDto> logbookEntriesToUpdate,CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();

                // fetch all entries
                var entryIds = logbookEntriesToUpdate.Select(e => e.Id).ToList();
                var existingEntries = await _repositoryManager.LogbookEntryRepository.GetByIdsAsync(entryIds, cancellationToken);
                if (!existingEntries.Any())
                {
                    return ApiResponse<IEnumerable<LogbookEntryDto>>.Failure(404, "No entries found for the provided IDs");
                }
                var entryDictionary = existingEntries.ToDictionary(e => e.Id);
                var updatedEntries = new List<LogbookEntry>();
                var failedEntries = new List<(LogbookEntryForUpdateDto Entry, string Reason)>();

                foreach (var entryDto in logbookEntriesToUpdate)
                {
                    if (!entryDictionary.TryGetValue(entryDto.Id, out var existingEntry))
                    {
                        failedEntries.Add((entryDto, "Entry not found"));
                        continue;
                    }
                    // Check if the user has permission to update this entry
                    if (existingEntry.UserId != userId)
                    {
                        failedEntries.Add((entryDto, "You do not have permission to update this entry"));
                        continue;
                    }
                    existingEntry.Activity = entryDto.Activity;
                    existingEntry.Description = entryDto.Description;
                    existingEntry.LastUpdatedDate = DateTime.UtcNow;

                    updatedEntries.Add(existingEntry);
                }

                if (updatedEntries.Any())
                {
                    _repositoryManager.LogbookEntryRepository.UpdateBatch(updatedEntries);
                    await _repositoryManager.Save();
                    // invalidate cache
                    InvalidateUserCache(userId);
                    foreach (var entry in updatedEntries)
                    {
                        InvalidateEntryCache(userId, entry.Id);
                    }
                    _logger.LogInformation("{Count} entries updated for user {UserId}", updatedEntries.Count, userId);
                }

                var successfulEntries = updatedEntries.Select(e => e.ToDto()).ToList();

                if (!failedEntries.Any())
                {
                    return ApiResponse<IEnumerable<LogbookEntryDto>>.Success(200, successfulEntries, "All entries updated successfully");
                }
                else
                {
                    var message = $"{successfulEntries.Count} entries updated successfully. {failedEntries.Count} entries failed.";
                    var errorDetails = failedEntries.Select(f => $"Entry with ID {f.Entry.Id}: {f.Reason}").ToList();
                    return ApiResponse<IEnumerable<LogbookEntryDto>>.PartialSuccess(207, successfulEntries, message, errorDetails);
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while updating user logbook entries: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<IEnumerable<LogbookEntryDto>>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }

        public async Task<ApiResponse<object>> DeleteEntriesAsync(IEnumerable<Guid> entryIds, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                var entriesToDelete = await _repositoryManager.LogbookEntryRepository.GetByIdsAsync(entryIds, cancellationToken);

                if (!entriesToDelete.Any())
                    return ApiResponse<object>.Failure(404, "No entries found for deletion");

                // Check if all entries belong to the current user
                if (entriesToDelete.Any(e => e.UserId != userId))
                    return ApiResponse<object>.Failure(403, "You don't have permission to delete one or more of these entries");

                _repositoryManager.LogbookEntryRepository.RemoveBatch(entriesToDelete);
                await _repositoryManager.Save();
                // Invalidate cache
                InvalidateUserCache(userId);
                foreach (var entryId in entryIds)
                {
                    InvalidateEntryCache(userId, entryId);
                }
                _logger.Log(LogLevel.Information, "{Count} entries deleted for user {UserId}", entriesToDelete.Count(), userId);
                return ApiResponse<object>.Success(204, null);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while deleting user logbook entries: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<object>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }

        #region Private methods
        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        private void InvalidateUserCache(string userId)
        {
            var allEntriesCacheKey = $"LogbookEntries_{userId}";
            _memoryCache.Remove(allEntriesCacheKey);
        }

        private void InvalidateEntryCache(string userId, Guid entryId)
        {
            var entryCacheKey = $"LogbookEntry_{userId}_{entryId}";
            _memoryCache.Remove(entryCacheKey);
        }

        #endregion
    }
}
