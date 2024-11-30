using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain;
using Domain.Entities;
using AutoMapper;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using DataAccess.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using Domain.ViewModels.Product;
using DataAccess.Commons;
using DataAccess.Interfaces;
using DataAccess.Utils;

namespace JWTDemo.Controllers.v5
{
    [ApiVersion(5)]
    [Route("api/v{v:apiVersion}/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IClaimsService _claimService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductsController(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<ChatHub> hubContext, IClaimsService claimService, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubContext = hubContext;
            _claimService = claimService;
            _userManager = userManager;
        }



        // GET: api/Products
        /// <summary>
        /// Get a list of Product
        /// </summary>
        /// <returns>A list of ProductDTO</returns>
        [MapToApiVersion(5)]
        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<Pagination<ProductDTO>>> GetProducts([FromQuery] FilterProductModel filterProductModel, int pageIndex = 0, int pageSize = 20)
        {
            //add includes
            List<Expression<Func<Product, object>>> includes = new List<Expression<Func<Product, object>>>{
                                 x => x.Category,
                                    };

            //add filters
            var filter = new List<Expression<Func<Product, bool>>>();
            filter.Add(x => !x.IsDeleted);
            //add keyword filter
            if (filterProductModel.Keyword != null) {
                string keywordLower = filterProductModel.Keyword.ToLower();
                filter.Add(x => x.Name.ToLower().Contains(keywordLower));
            }

            //add category filter
            try
            {
                if (filterProductModel.CategoryId != null && filterProductModel.CategoryId != "")
                {
                    filter.Add(x => x.CategoryId == Guid.Parse(filterProductModel.CategoryId));
                }
            }
            catch (Exception)
            {
               return BadRequest("Category Not found!");
            }

            //add price range filter
            if (filterProductModel.MinPrice != null)
            {
                filter.Add(x => x.Price >= filterProductModel.MinPrice);
            }
            if (filterProductModel.MaxPrice != null)
            {
                filter.Add(x => x.Price <= filterProductModel.MaxPrice);
            }

            var finalFilter = filter.Aggregate((current, next) => current.AndAlso(next));

            var products = await _unitOfWork.ProductRepository.GetAsyncPagination(includes: includes, pageIndex: pageIndex, pageSize: pageSize, expression: finalFilter);
            var productDTOs = _mapper.Map<Pagination<ProductDTO>>(products);
            Log.Information("Result: {@productDTOs}", productDTOs);
            return Ok(productDTOs);
        }

        // GET: api/Products/5
        /// <summary>
        /// Get a specific Product
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A specific ProductDTO</returns>
        [MapToApiVersion(5)]
        [HttpGet("Customer/{id}")]
        [Produces("application/json")]
        public ActionResult<ProductDTO> GetProductCustomer(Guid id)
        {
            List<Expression<Func<Product, object>>> includes = new List<Expression<Func<Product, object>>>{
                                 x => x.Category,
                                    };
            var product = _unitOfWork.ProductRepository
                .GetAsync(expression: x => x.Id == id, includes: includes).Result.FirstOrDefault();

            if (product == null)
            {
                Log.Information("Not found Product.");
                return NotFound("Not found Product.");
            }
            var productDTO = _mapper.Map<ProductDTO>(product);
            Log.Information("Result: {@productDTO}", productDTO);
            return Ok(productDTO);
        }

        // GET: api/Products/5
        /// <summary>
        /// Get a specific Product
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A specific ProductDTO</returns>
        [MapToApiVersion(5)]
        [HttpGet("{id}")]
        [Produces("application/json")]
        public ActionResult<ProductRequestDTO> GetProduct(Guid id)
        {
            List<Expression<Func<Product, object>>> includes = new List<Expression<Func<Product, object>>>{
                                 x => x.Category,
                                    };
            var product = _unitOfWork.ProductRepository
                .GetAsync(expression: x => x.Id == id, includes: includes).Result.FirstOrDefault();

            if (product == null)
            {
                Log.Information("Not found Product.");
                return NotFound("Not found Product.");
            }
            var productRequestDTO = _mapper.Map<ProductRequestDTO>(product);
            Log.Information("Result: {@productDTO}", productRequestDTO);
            return Ok(productRequestDTO);
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
        [MapToApiVersion(5)]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> PutProduct(Guid id, ProductRequestDTO productDTO)
        {
            var product = await _unitOfWork.ProductRepository.GetById(id);
            if (product == null)
            {
                Log.Information("Not found Product.");
                return NotFound("Not Found Product.");
            }

            var result = _mapper.Map<Product>(productDTO);
            result.Id = product.Id;

            try
            {
                _unitOfWork.ProductRepository.Update(result);
                await _unitOfWork.Complete();
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest();
            }
            var userId = _claimService.GetCurrentUserId.ToString();
            var user = await _userManager.FindByIdAsync(userId);

            Log.Information($"Admin {user.Email} updated product {product.Name} successfully!");
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", userId, $"Admin {user.Email} updated product {product.Name} successfully!");
            return Ok(new { msg = "Updated product successfully!" });
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
        [MapToApiVersion(5)]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<Product>> PostProduct(ProductRequestDTO productDTO)
        {
            if (productDTO == null) { return BadRequest(); }
            var product = _mapper.Map<Product>(productDTO);
            await _unitOfWork.ProductRepository.Add(product);
            await _unitOfWork.Complete();
            var userId = _claimService.GetCurrentUserId.ToString();
            var user = await _userManager.FindByIdAsync(userId);

            Log.Information($"Admin {user.Email} created product {product.Name} successfully!");
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", userId, $"Admin {user.Email} created product {product.Name} successfully!");
            return Ok(new { msg = "Created product successfully!" });
        }

        // DELETE: api/Products/5

        /// <summary>
        /// Deletes a specific Product
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MapToApiVersion(5)]
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var product = await _unitOfWork.ProductRepository.GetById(id);
            if (product == null)
            {
                Log.Information("Not found Product.");
                return NotFound("Not found Product.");
            }

            _unitOfWork.ProductRepository.Remove(product);
            await _unitOfWork.Complete();
            var userId = _claimService.GetCurrentUserId.ToString();
            var user = await _userManager.FindByIdAsync(userId);

            Log.Information($"Admin {user.Email} deleted product {product.Name} successfully!");
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", userId, $"Admin {user.Email} deleted product {product.Name} successfully!");
            return Ok(new { msg = "Removed product successfully!" });
        }
    }
}
