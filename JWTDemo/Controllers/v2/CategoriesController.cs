using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Domain.ViewModels.Category;
using DataAccess.Interfaces;

namespace JWTDemo.Controllers.v2
{
    [ApiVersion(2)]
    [Route("api/v{v:apiVersion}/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoriesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [MapToApiVersion(2)]
        // GET: api/Categories
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return Ok(await _unitOfWork.CategoryRepository.GetAll());
        }

        [MapToApiVersion(2)]
        // GET: api/Categories/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Category>> GetCategory(Guid id)
        {
            var category = await _unitOfWork.CategoryRepository.GetById(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        [MapToApiVersion(2)]
        // PUT: api/Categories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> PutCategory(Guid id, CategoryDTO categoryDTO)
        {
            var category = await _unitOfWork.CategoryRepository.GetById(id);
            if (category == null)
            {
                return NotFound("Not Found Category");
            }

            category.Name = categoryDTO.Name;

            _unitOfWork.CategoryRepository.Update(category);

            try
            {
                await _unitOfWork.Complete();
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest();
            }

            return NoContent();
        }

        // POST: api/Categories
        [MapToApiVersion(2)]
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<Category>> PostCategory(CategoryDTO categoryDTO)
        {
            if (categoryDTO == null) { return BadRequest(); }
            Category category = new Category()
            {
                Name = categoryDTO.Name,
            };
            await _unitOfWork.CategoryRepository.Add(category);
            await _unitOfWork.Complete();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }

        // DELETE: api/Categories/5
        [MapToApiVersion(2)]
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var category = await _unitOfWork.CategoryRepository.GetById(id);
            if (category == null)
            {
                return NotFound();
            }

            _unitOfWork.CategoryRepository.Remove(category);
            await _unitOfWork.Complete();

            return NoContent();
        }
    }
}
