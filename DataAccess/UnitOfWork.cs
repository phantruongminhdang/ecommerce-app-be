using DataAccess.Interfaces;
using Domain;
using Microsoft.EntityFrameworkCore.Storage;

namespace DataAccess.Service
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly ICustomerRepository _customerRepository;
        private IDbContextTransaction _transaction;

        public UnitOfWork(ApplicationDbContext context, IProductRepository productRepository, ICategoryRepository categoryRepository,
            IOrderRepository orderRepository, IOrderItemRepository orderItemRepository, ICartRepository cartRepository, 
            ICartItemRepository cartItemRepository, ICustomerRepository customerRepository)
        {
            _context = context;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _customerRepository = customerRepository;
        }

        public IProductRepository ProductRepository => _productRepository;

        public ICategoryRepository CategoryRepository => _categoryRepository;
        public IOrderItemRepository OrderItemRepository => _orderItemRepository;
        public IOrderRepository OrderRepository => _orderRepository;
        public ICartRepository CartRepository => _cartRepository;
        public ICartItemRepository CartItemRepository => _cartItemRepository;  
        public ICustomerRepository CustomerRepository => _customerRepository;

        public async Task<int> Complete() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();

        public void BeginTransaction()
        {
            _transaction = _context.Database.BeginTransaction();
        }
        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                _transaction.Commit();
            }
            catch
            {
                _transaction.Rollback();
                throw;
            }
        }
        public void RollbackTransaction()
        {
            _transaction.Rollback();
        }
        public void ClearTrack()
        {
            _context.ChangeTracker.Clear();
        }
    }
}
