using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    /// <summary>
    /// Manages CRUD operations for reusable race categories. 
    /// </summary>
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context) { _context = context; }

        /// <summary>
        /// Lists all reusable RaceDay categories.
        /// </summary>
        /// <response code="200">Returns category list.</response>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategories()
        {
            return Ok(await _context.Categories.ToListAsync());
        }

        /// <summary>
        /// Gets one category by ID.
        /// </summary>
        /// <response code="200">Returns the category.</response>
        /// <response code="404">Category not found.</response>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound(new { message = "Category not found." });
            return Ok(category);
        }

        /// <summary>
        /// Creates a new event category.
        /// </summary>
        /// <response code="201">Category created.</response>
        /// <response code="409">Category name already exists.</response>
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateCategory(CategoryDto request)
        {
            if (await _context.Categories.AnyAsync(c => c.Name == request.Name))
                return Conflict(new { message = "A category with this name already exists." });

            var category = new Category
            {
                Name = request.Name,
                Description = request.Description
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return StatusCode(201, category);
        }

        /// <summary>
        /// Updates a category.
        /// </summary>
        /// <response code="200">Category updated.</response>
        /// <response code="404">Category not found.</response>
        /// <response code="409">Category name already exists.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryDto request)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound(new { message = "Category not found." });

            if (await _context.Categories.AnyAsync(c => c.Name == request.Name && c.CategoryId != id))
                return Conflict(new { message = "Another category with this name already exists." });

            category.Name = request.Name;
            category.Description = request.Description;

            await _context.SaveChangesAsync();
            return Ok(category);
        }

        /// <summary>
        /// Deletes a category and its event-category links.
        /// </summary>
        /// <response code="204">Category deleted.</response>
        /// <response code="404">Category not found.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound(new { message = "Category not found." });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
