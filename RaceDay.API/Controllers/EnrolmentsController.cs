using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Route("api/events/{eventId}/enrolments")]
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context) { _context = context; }

        /// <summary>
        /// Enrols the logged-in participant in a selected event category.
        /// </summary>
        /// <response code="201">Enrolment created.</response>
        /// <response code="400">Invalid category for this event.</response>
        /// <response code="403">User is not a Participant.</response>
        /// <response code="404">Event not found.</response>
        /// <response code="409">Already enrolled, or registration is closed.</response>
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> EnrolInEvent(int eventId, EnrolmentDto request)
        {
            var participantId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var eventEntity = await _context.Events.FindAsync(eventId);
            if (eventEntity == null) return NotFound(new { message = "Event not found." });

            if (eventEntity.RegistrationDeadline < DateTime.UtcNow)
                return Conflict(new { message = "Registration deadline has passed." });

            // Validate the Category is actually linked to this Event (Composite Key Check)
            var isValidCategory = await _context.EventCategories
                .AnyAsync(ec => ec.EventId == eventId && ec.CategoryId == request.CategoryId);

            if (!isValidCategory) return BadRequest(new { message = "Invalid category for this event." });

            // Check for duplicate enrolment (Unique constraint check)
            var existing = await _context.Enrolments
                .AnyAsync(e => e.EventId == eventId && e.ParticipantId == participantId);

            if (existing) return Conflict(new { message = "You are already enrolled in this event." });

            var enrolment = new Enrolment
            {
                ParticipantId = participantId,
                EventId = eventId,
                CategoryId = request.CategoryId,
                Status = "Active"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return StatusCode(201, enrolment);
        }

        /// <summary>
        /// Lists all participant enrolments for an event.
        /// </summary>
        /// <response code="200">Returns enrolment list.</response>
        /// <response code="403">User is not an Organiser.</response>
        /// <response code="404">Event not found.</response>
        [HttpGet]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            if (!await _context.Events.AnyAsync(e => e.EventId == eventId))
                return NotFound(new { message = "Event not found." });

            var enrolments = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .Where(e => e.EventId == eventId)
                .Select(e => new
                {
                    e.EnrolmentId,
                    ParticipantName = e.Participant!.FirstName + " " + e.Participant.LastName,
                    CategoryName = e.Category!.Name,
                    e.EnrolmentDate,
                    e.Status
                }).ToListAsync();

            return Ok(enrolments);
        }
    }
}