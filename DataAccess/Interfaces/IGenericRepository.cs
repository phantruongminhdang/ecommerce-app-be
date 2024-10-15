using DataAccess.Commons;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetById(Guid id);
        Task<IEnumerable<T>> GetAll();
        Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>> expression = null,
            List<Expression<Func<T, object>>> includes = null);
        Task<Pagination<T>> GetAsyncPagination(
            Expression<Func<T, bool>> expression = null, 
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, 
            bool isDisableTracking = true, 
            bool isTakeAll = false, 
            int pageSize = 0, int pageIndex = 0, 
            List<Expression<Func<T, object>>> includes = null);

        Task<int> CountAsync(Expression<Func<T, bool>> expression = null);
        Task<IEnumerable<T>> Find(Expression<Func<T, bool>> expression = null);
        Task Add(T entity);
        Task AddRange(IEnumerable<T> entities);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        void Modified(T entity);
    }
}
