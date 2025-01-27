namespace Si.EntityFramework.Extension.Abstraction
{
    public interface IUnitOfWork
    {
        Task<int> CommitAsync();
        void Dispose();
        IRepository<T> GetRepository<T>() where T : class;
        void Rollback();
    }
}