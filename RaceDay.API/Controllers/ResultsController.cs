using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context) { _context = context; }

        /// <summary>
        /// Records a result for an enrolment belonging to the event.
        /// </summary>
        /// <response code="201">Result created.</response>
        /// <response code="400">Invalid result data.</response>
        /// <response code="403">User is not the Organiser for this event.</response>
        /// <response code="404">Event or Enrolment not found.</response>
        /// <response code="409">Result already exists for this enrolment.</response>
        [HttpPost("events/{eventId}/results")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CaptureResult(int eventId, ResultDto request)
        {
            var organiserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.OrganiserId == organiserId);

            if (eventEntity == null) return Forbid();

            var enrolment = await _context.Enrolments
                .FirstOrDefaultAsync(e => e.EnrolmentId == request.EnrolmentId && e.EventId == eventId);

            if (enrolment == null) return NotFound(new { message = "Enrolment not found for this event." });

            if (await _context.Results.AnyAsync(r => r.EnrolmentId == request.EnrolmentId))
                return Conflict(new { message = "A result already exists for this enrolment." });

            var result = new Result
            {
                EnrolmentId = request.EnrolmentId,
                FinishTime = request.FinishTime,
                Position = request.Position,
                ResultStatus = request.ResultStatus
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return StatusCode(201, result);
        }

        /// <summary>
        /// Lists the authenticated participant's personal results.
        /// </summary>
        /// <response code="200">Returns personal results.</response>
        /// <response code="403">User is not a Participant.</response>
        [HttpGet("users/me/results")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var participantId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Event)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Category)
                .Where(r => r.Enrolment!.ParticipantId == participantId)
                .Select(r => new
                {
                    EventName = r.Enrolment!.Event!.EventName,
                    CategoryName = r.Enrolment.Category!.Name,
                    r.FinishTime,
                    r.Position,
                    r.ResultStatus,
                    r.RecordedAt
                }).ToListAsync();

            return Ok(results);
        }
    }
}
