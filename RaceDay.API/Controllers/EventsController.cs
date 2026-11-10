using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    /// <summary>
    /// Manages CRUD operations for race events.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

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
                    OrganiserName = e.Organiser != null ? e.Organiser.FirstName + " " + e.Organiser.LastName : "Unknown"
                }).ToListAsync();

            return Ok(events);
        }

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
                OrganiserName = ev.Organiser != null ? ev.Organiser.FirstName + " " + ev.Organiser.LastName : "Unknown"
            });
        }

        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent([FromForm] EventDto request)
        {
            if (request.RegistrationDeadline > request.EventDate)
                return BadRequest(new { message = "Registration deadline must be before the event date." });

            var organiserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var newEvent = new Event
            {
                OrganiserId = organiserId,
                EventName = request.EventName,
                Description = request.Description ?? string.Empty,
                Location = request.Location,
                EventDate = request.EventDate,
                RegistrationDeadline = request.RegistrationDeadline,
                Status = request.Status
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                newEvent.EventId,
                newEvent.EventName,
                newEvent.Description,
                newEvent.Location,
                newEvent.EventDate,
                newEvent.RegistrationDeadline,
                newEvent.Status
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] EventDto request)
        {
            var organiserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound(new { message = "Event not found." });
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
