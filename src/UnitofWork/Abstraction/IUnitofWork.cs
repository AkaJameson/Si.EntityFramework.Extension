using Si.EntityFramework.Extension.Database;

namespace Si.EntityFramework.Extension.UnitofWork.Abstraction
{
    public interface IUnitOfWork<TContext> where TContext : ApplicationDbContext
    {
        void BeginTransaction();
        void ClearChangeTracker();
        Task<int> CommitAsync();
        Task CommitTransactionAsync();
        void Dispose();
        Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters);
        IRepository<T> GetRepository<T>() where T : class;
        void Rollback();
        Task RollbackTransactionAsync();
        Task<IEnumerable<T>> SqlQueryAsync<T>(string sql, params object[] parameters) where T : class, new();
    }
}