using LogBook_API.Contracts;
using LogBook_API.Persistence.RequestParameters;
using LogBook_API.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LogBook_API.Presentation.Controllers
{
    [Route("api/logbook")]
    [ApiController]
    public class LogbookController : ControllerBase
    {
        private readonly ILogbookEntryService _logbookEntryService;
        public LogbookController(ILogbookEntryService logbookEntryService)
        {
            _logbookEntryService = logbookEntryService; 
        }
        /// <summary>
        /// Retrieves all logbook entries. Only accessible to users with 'Administrator' role.
        /// </summary>
        /// <returns>A list of logbook entries if successful, or an error message if a failure occurs.</returns>
        /// <response code="200">Returns a list of logbook entries.</response>
        /// <response code="500">Returns an error message if an unexpected error occurs.</response>
        [Authorize(Roles ="Administrator")]
        [HttpGet("entries")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LogbookEntryDto>>), StatusCodes.Status200OK)]  // For successful retrieval of entries
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]  // For server errors
        public async Task<IActionResult> GetAllEntries()
        {
            var result = await _logbookEntryService.GetAllAsync();
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Retrieves all logbook entries for an authenticated user
        /// </summary>
        /// <returns>A list of logbook entries if successful, or an error message if a failure occurs.</returns>
        /// <response code="200">Returns a list of logbook entries.</response>
        /// <response code="500">Returns an error message if an unexpected error occurs.</response>
        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LogbookEntryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUserEntries()
        {
            var result = await _logbookEntryService.GetAllEntriesAsync();
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Retrieves a specific logbook entry by its ID. Only accessible to authenticated users.
        /// </summary>
        /// <param name="entryId">The unique identifier of the logbook entry to retrieve.</param>
        /// <returns>The details of the logbook entry if successful, or an error message if a failure occurs.</returns>
        /// <response code="200">Returns the details of the logbook entry.</response>
        /// <response code="400">Returns an error message if the logbook entry with the specified ID does not exist.</response>
        /// <response code="500">Returns an error message if an unexpected error occurs.</response>
        [Authorize] 
        [HttpGet]
        [Route("{entryId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LogbookEntryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>),StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEntryById(Guid entryId)
        {
            var result = await _logbookEntryService.GetEntryByIdAsync(entryId);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Creates a new logbook entry.
        /// </summary>
        /// <param name="logbookEntry">The details of the logbook entry to create.</param>
        /// <returns>Returns a created logbook entry if successful, or an error message if validation fails or an error occurs.</returns>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LogbookEntryDto>), StatusCodes.Status201Created)]  // For successful creation
        [ProducesResponseType(typeof(ApiResponse<ModelStateDictionary>), StatusCodes.Status400BadRequest)]  // For bad request (invalid payload)
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]  // For bad request 
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]  // For unexpected errors
        public async Task<IActionResult> CreateLogbookEntry(LogbookEntryForCreationDto logbookEntry)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ModelStateDictionary>.Success(400, ModelState, "Invalid payload"));
            }
            var result = await _logbookEntryService.CreateEntryAsync(logbookEntry);
            if(result.StatusCode == 201)
                return CreatedAtAction(nameof(GetEntryById), new { entryId = result.Data.Id }, result.Data);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Updates a logbook entry. Only accessible to the user who created the entry.
        /// </summary>
        /// <param name="entryId">The unique identifier of the logbook entry to update.</param>
        /// <param name="logbookEntryForUpdateDto">The data required to update the logbook entry.</param>
        /// <returns>The updated logbook entry if successful, or an error message if a failure occurs.</returns>
        [Authorize]
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<LogbookEntryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<ModelStateDictionary>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> UpdateLogbookEntry([FromQuery] Guid entryId, LogbookEntryForUpdateDto logbookEntryForUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ModelStateDictionary>.Success(400, ModelState, "Invalid payload"));
            }
            var result = await _logbookEntryService.UpdateEntryAsync(entryId, logbookEntryForUpdateDto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Deletes a logbook entry.
        /// </summary>
        /// <param name="entryId">The ID of the logbook entry to delete.</param>
        /// <returns>Returns no content if deletion is successful, or an error message if validation fails or an error occurs.</returns>
        [Authorize]
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]  // For successful deletion with no content
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]  // For bad request (invalid entry ID)
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]  // For forbidden access (user not authorized to delete)
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]  // For unexpected errors
        public async Task<IActionResult> DeleteLogbookEntry([FromQuery] Guid entryId)
        {
            var result = await _logbookEntryService.DeleteEntryAsync(entryId);
            if(result.StatusCode == 204)
                return NoContent();
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Creates multiple logbook entries. Only accessible to authorized users.
        /// </summary>
        /// <param name="logbookEntries">A collection of logbook entries to be created.</param>
        /// <returns>A list of successfully created entries, or a partial success message with details of any failed entries.</returns>
        [Authorize]
        [HttpPost("create-multiple-entries")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LogbookEntryDto>>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<ModelStateDictionary>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LogbookEntryDto>>), StatusCodes.Status207MultiStatus)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateMultipleLogbookEntries(IEnumerable<LogbookEntryForCreationDto> logbookEntries)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ModelStateDictionary>.Success(400, ModelState, "Invalid payload"));
            }
            var result = await _logbookEntryService.CreateEntriesAsync(logbookEntries);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Updates multiple logbook entries. Only accessible to authorized users.
        /// </summary>
        /// <param name="logbookEntries">A collection of logbook entries with updated details.</param>
        /// <returns>A list of successfully updated entries, or a partial success message with details of any failed updates.</returns>
        [Authorize]
        [HttpPut("update-multiple-entries")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LogbookEntryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ModelStateDictionary>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LogbookEntryDto>>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LogbookEntryDto>>), StatusCodes.Status207MultiStatus)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMultipleLogbookEntries(IEnumerable<LogbookEntryForUpdateDto> logbookEntries)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ModelStateDictionary>.Failure(400, ModelState, "Invalid payload"));
            }
            var result = await _logbookEntryService.UpdateMultipleEntriesAsync(logbookEntries);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Deletes multiple logbook entries. Only accessible to authorized users.
        /// </summary>
        /// <param name="logbookEntryIds">A collection of logbook entry IDs to be deleted.</param>
        /// <returns>A success status if all entries are deleted, or an error message if some entries could not be deleted.</returns>
        [Authorize]
        [Authorize]
        [HttpDelete("delete-multiple-entries")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<ModelStateDictionary>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LogbookEntryDto>>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMultipleLogbookEntries(IEnumerable<Guid> logbookEntryIds)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ModelStateDictionary>.Success(400, ModelState, "Invalid payload"));
            }
            var result = await _logbookEntryService.DeleteEntriesAsync(logbookEntryIds);
            if (result.StatusCode == 204)
                return NoContent();
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Downloads logbook entries as a CSV file. The file is generated based on the provided date filter parameters.
        /// </summary>
        /// <param name="dateFilterParameter">The date filter parameters to apply when retrieving logbook entries.</param>
        /// <returns>A CSV file containing the logbook entries if successful. Returns a 500 status code if an error occurs.</returns>
        [Authorize]
        [Produces("text/csv")]
        [HttpGet]
        [Route("download-csv")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<IActionResult> DownloadEntriesAsCSV([FromQuery] DateFilterParameter dateFilterParameter)
        {
            // Fetch entries and generate csv file
            var csvFile = await _logbookEntryService.GetEntriesAsCSVFormat(dateFilterParameter);
            // Return the CSV file as a downloadable file
            return File(csvFile, "text/csv", "LogbookEntries.csv");
        }
    }
}
