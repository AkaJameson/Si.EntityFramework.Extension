namespace Si.Framework.EntityFramework.UnitofWork
{
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// 获取指定类型的仓储
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <returns>指定类型的仓储</returns>
        IRepository<T> GetRepository<T>() where T : class;

        /// <summary>
        /// 异步提交所有更改
        /// </summary>
        /// <returns>受影响的行数</returns>
        Task<int> CommitAsync();

        /// <summary>
        /// 异步执行事务
        /// </summary>
        /// <param name="action">事务操作</param>
        Task ExecuteTransactionAsync(Func<Task> action);

        Task ExecuteTransactionWithRetryAsync(Func<Task> action, int retryCount = 3);

        /// <summary>
        /// 回滚未提交的更改
        /// </summary>
        void Rollback();
    }
}
