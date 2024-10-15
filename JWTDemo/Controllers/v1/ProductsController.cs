using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using AutoMapper;
using System.Linq.Expressions;
using Asp.Versioning;
using Domain.ViewModels.Product;
using DataAccess.Interfaces;

namespace JWTDemo.Controllers.v1
{
    [ApiVersion(1)]
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
        [MapToApiVersion(1)]
        [HttpGet]
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
        [MapToApiVersion(1)]
        [HttpGet("{id}")]
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
    }
}
