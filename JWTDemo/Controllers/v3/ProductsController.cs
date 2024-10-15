using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain;
using Domain.Entities;
using AutoMapper;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Domain.ViewModels.Product;
using DataAccess.Interfaces;

namespace JWTDemo.Controllers.v3
{
    [ApiVersion(3)]
    [Route("api/v{v:apiVersion}/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: api/Products
        /// <summary>
        /// Get a list of Product
        /// </summary>
        /// <returns>A list of ProductDTO</returns>
        [MapToApiVersion(3)]
        [HttpGet]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts()
        {
            List<Expression<Func<Product, object>>> includes = new List<Expression<Func<Product, object>>>{
                                 x => x.Category,
                                    };
            var products = await _unitOfWork.ProductRepository.GetAsync(includes: includes);
            var productDTOs = _mapper.Map<IEnumerable<ProductDTO>>(products);
            return Ok(productDTOs);
        }

        // GET: api/Products/5
        /// <summary>
        /// Get a specific Product
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A specific ProductDTO</returns>
        [MapToApiVersion(3)]
        [HttpGet("{id}")]
        [Authorize]
        [Produces("application/json")]
        public ActionResult<ProductDTO> GetProduct(Guid id)
        {
            List<Expression<Func<Product, object>>> includes = new List<Expression<Func<Product, object>>>{
                                 x => x.Category,
                                    };
            var product = _unitOfWork.ProductRepository
                .GetAsync(expression: x => x.Id == id, includes: includes).Result.FirstOrDefault();

            if (product == null)
            {
                return NotFound();
            }
            var productDTO = _mapper.Map<ProductDTO>(product);
            return productDTO;
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// Creates a Product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="productDTO"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /api/Products/{id}
        ///     {
        ///       "categoryName": "category #1",
        ///       "name": "product #1",
        ///       "description": "string",
        ///       "price": 1000,
        ///       "code": "string"
        ///     }
        ///
        /// </remarks>
        /// <response code="204">No content</response>
        /// <response code="400">If the product is null</response>
        /// <response code="404">If the product is not found</response>
        [MapToApiVersion(3)]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> PutProduct(Guid id, ProductDTO productDTO)
        {
            var product = await _unitOfWork.ProductRepository.GetById(id);
            if (product == null)
            {
                return NotFound("Not Found Product");
            }

            var result = _mapper.Map<Product>(productDTO);
            result.Id = id;

            try
            {
                _unitOfWork.BeginTransaction();
                _unitOfWork.ProductRepository.Update(product);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest();
            }

            return NoContent();
        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// Creates a Product
        /// </summary>
        /// <param name="productDTO"></param>
        /// <returns>A newly created Product</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/Products
        ///     {
        ///       "categoryName": "category #1",
        ///       "name": "product #1",
        ///       "description": "string",
        ///       "price": 1000,
        ///       "code": "string"
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Returns the newly created product</response>
        /// <response code="400">If the product is null</response>
        [MapToApiVersion(3)]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<Product>> PostProduct(ProductDTO productDTO)
        {
            if (productDTO == null) { return BadRequest(); }
            var product = _mapper.Map<Product>(productDTO);
            await _unitOfWork.ProductRepository.Add(product);
            await _unitOfWork.Complete();

            return CreatedAtAction("GetProducts", new { id = product.Id }, product);
        }

        // DELETE: api/Products/5

        /// <summary>
        /// Deletes a specific Product
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MapToApiVersion(3)]
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var product = await _unitOfWork.ProductRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            _unitOfWork.ProductRepository.Remove(product);
            await _unitOfWork.Complete();

            return NoContent();
        }
    }
}
