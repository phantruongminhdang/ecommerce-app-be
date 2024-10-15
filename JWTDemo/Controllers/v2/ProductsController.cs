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

namespace JWTDemo.Controllers.v2
{
    [ApiVersion(2)]
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
        [MapToApiVersion(2)]
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
        [MapToApiVersion(2)]
        [HttpGet("{id}")]
        [Authorize]
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
        [MapToApiVersion(2)]
        [HttpPut("{id}")]
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
        [MapToApiVersion(2)]
        [HttpPost]
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
        [MapToApiVersion(2)]
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
