using DataAccess.Commons;
using DataAccess.Interfaces;
using Domain;
using Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Service
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected DbSet<T> _dbSet;
        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task Add(T entity) => await _context.Set<T>().AddAsync(entity);

        public async Task AddRange(IEnumerable<T> entities) => await _context.Set<T>().AddRangeAsync(entities);

        public async Task<IEnumerable<T>> Find(Expression<Func<T, bool>> expression) => await _context.Set<T>().Where(expression).ToListAsync();

        public async Task<IEnumerable<T>> GetAll() => await _context.Set<T>().ToListAsync();

        public async Task<T?> GetById(Guid id) => await _context.Set<T>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

        public void Remove(T entity) => _context.Set<T>().Remove(entity);

        public void RemoveRange(IEnumerable<T> entities) => _context.Set<T>().RemoveRange(entities);

        public void Update(T entity)
        {
            _context.Update(entity);
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            _context.UpdateRange(entities);
        }

        public async Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>> expression = null,
            List<Expression<Func<T, object>>> includes = null)
        {
            IQueryable<T> query = _dbSet;
            IEnumerable<T> entities;
            if (includes != null && includes.Any())
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            if (expression != null)
                query = query.Where(expression);
            entities = await query.ToListAsync();
            return entities;
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> expression = null)
        {
            return expression == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(expression);
        }

        public async Task<Pagination<T>> GetAsyncPagination(
            Expression<Func<T, bool>> expression = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            bool isDisableTracking = true,
            bool isTakeAll = false,
            int pageSize = 0,
            int pageIndex = 0,
            List<Expression<Func<T, object>>> includes = null)
        {
            IQueryable<T> query = _dbSet;
            var paginationResult = new Pagination<T>();
            paginationResult.PageIndex = pageIndex;
            if (pageSize == 0)
                paginationResult.PageSize = await CountAsync(expression);
            else
                paginationResult.PageSize = pageSize;
            paginationResult.TotalItemsCount = await CountAsync(expression);
            if (includes != null && includes.Any())
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            if (expression != null)
                query = query.Where(expression);
            if (isDisableTracking is true)
                query = query.AsNoTracking();
            if (isTakeAll is true)

            {
                if (orderBy != null)
                    paginationResult.Items = await orderBy(query).ToListAsync();
                else
                    paginationResult.Items = await query.ToListAsync();
            }
            else
            {
                if (pageIndex < 0 || pageSize < 0)
                {
                    throw new Exception("Số trang và sô lượng trong trang phải lớn hơn 0.");
                }
                if (orderBy == null)
                    paginationResult.Items = await query.Skip(pageSize * pageIndex).Take(pageSize).ToListAsync();
                else
                    paginationResult.Items = await orderBy(query).Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();
            }
            return paginationResult;
        }

        public void Modified(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }
    }
}
