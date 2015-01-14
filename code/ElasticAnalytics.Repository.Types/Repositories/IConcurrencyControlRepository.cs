namespace ElasticAnalytics.Repository.Types.Repositories
{
    using ElasticAnalytics.Service.Types;

    public interface IConcurrencyControlRepository<T> : IRepository<T>
        where T : IVersionable
    {
        bool SaveOptimistically(T entity, ISystemContext ctx);

        bool DeleteOptimistically(object entityId, long version, ISystemContext ctx);
    }
}