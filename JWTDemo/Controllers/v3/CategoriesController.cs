using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Domain.ViewModels.Category;
using DataAccess.Interfaces;

namespace JWTDemo.Controllers.v3
{
    [ApiVersion(3)]
    [Route("api/v{v:apiVersion}/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoriesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Get a list of Category
        /// </summary>
        /// <returns>A list of Category</returns>
        [MapToApiVersion(3)]
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return Ok(await _unitOfWork.CategoryRepository.GetAll());
        }

        /// <summary>
        /// Get a specific Category
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A specific Category</returns>
        [MapToApiVersion(3)]
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

        /// <summary>
        /// Update a Category
        /// </summary>
        /// <param name="id"></param>
        /// <param name="categoryDTO"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /api/Categories/{id}
        ///     {
        ///       "name": "caategory #1"
        ///     }
        ///
        /// </remarks>
        /// <response code="204">No content</response>
        /// <response code="400">If the category is null</response>
        /// <response code="404">If the category is not found</response>
        [MapToApiVersion(3)]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// Creates a Category
        /// </summary>
        /// <param name="categoryDTO"></param>
        /// <returns>A newly created Category</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/Categories
        ///     {
        ///       "name": "caategory #1"
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Returns the newly created category</response>
        /// <response code="400">If the category is null</response>
        [MapToApiVersion(3)]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        /// <summary>
        /// Deletes a specific Category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MapToApiVersion(3)]
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
