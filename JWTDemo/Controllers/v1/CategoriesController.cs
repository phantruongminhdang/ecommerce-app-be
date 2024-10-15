using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Asp.Versioning;
using DataAccess.Interfaces;

namespace JWTDemo.Controllers.v1
{
    [ApiVersion(1)]
    [Route("api/v{v:apiVersion}/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoriesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [MapToApiVersion(1)]
        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return Ok(await _unitOfWork.CategoryRepository.GetAll());
        }

        [MapToApiVersion(1)]
        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(Guid id)
        {
            var category = await _unitOfWork.CategoryRepository.GetById(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

    }
}
