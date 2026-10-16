using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context) { _context = context; }

        /// <summary>
        /// Lists all events available in RaceDay.
        /// </summary>
        /// <response code="200">Returns event list.</response>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .Include(e => e.Organiser)
                .Select(e => new
                {
                    e.EventId,
                    e.EventName,
                    e.Description,
                    e.Location,
                    e.EventDate,
                    e.RegistrationDeadline,
                    e.Status,
                    OrganiserName = e.Organiser!.FirstName + " " + e.Organiser.LastName
                }).ToListAsync();

            return Ok(events);
        }

        /// <summary>
        /// Gets one event and its details.
        /// </summary>
        /// <response code="200">Returns the event.</response>
        /// <response code="404">Event not found.</response>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEvent(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null) return NotFound(new { message = "Event not found." });

            return Ok(new
            {
                ev.EventId,
                ev.EventName,
                ev.Description,
                ev.Location,
                ev.EventDate,
                ev.RegistrationDeadline,
                ev.Status,
                OrganiserName = ev.Organiser!.FirstName + " " + ev.Organiser.LastName
            });
        }

        /// <summary>
        /// Creates an event owned by the logged-in organiser.
        /// </summary>
        /// <response code="201">Event created.</response>
        /// <response code="400">Invalid dates/data.</response>
        /// <response code="403">User is not an Organiser.</response>
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent(EventDto request)
        {
            // Check: Registration deadline must be before event date
            if (request.RegistrationDeadline > request.EventDate)
                return BadRequest(new { message = "Registration deadline must be before the event date." });

            var organiserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var newEvent = new Event
            {
                OrganiserId = organiserId,
                EventName = request.EventName,
                Description = request.Description,
                Location = request.Location,
                EventDate = request.EventDate,
                RegistrationDeadline = request.RegistrationDeadline,
                Status = request.Status
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return StatusCode(201, newEvent);
        }

        /// <summary>
        /// Updates an event owned by the organiser.
        /// </summary>
        /// <response code="200">Event updated.</response>
        /// <response code="403">User is not the owner.</response>
        /// <response code="404">Event not found.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, EventDto request)
        {
            var organiserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound(new { message = "Event not found." });

            // CRITICAL: Only the owner can update their event
            if (ev.OrganiserId != organiserId) return Forbid();

            if (request.RegistrationDeadline > request.EventDate)
                return BadRequest(new { message = "Registration deadline must be before the event date." });

            ev.EventName = request.EventName;
            ev.Description = request.Description;
            ev.Location = request.Location;
            ev.EventDate = request.EventDate;
            ev.RegistrationDeadline = request.RegistrationDeadline;
            ev.Status = request.Status;

            await _context.SaveChangesAsync();

            return Ok(ev);
        }

        /// <summary>
        /// Deletes an event owned by the organiser.
        /// </summary>
        /// <response code="204">Event deleted.</response>
        /// <response code="403">User is not the owner.</response>
        /// <response code="404">Event not found.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var organiserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound(new { message = "Event not found." });

            if (ev.OrganiserId != organiserId) return Forbid();

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}