namespace ElasticAnalytics.Repository.Types.Repositories
{
    using ElasticAnalytics.Service.Types;

    public interface IRepository<T> 
    {
        bool Create(T entity, ISystemContext ctx);

        T Get(object entityId, ISystemContext ctx);
        
        void Save(T entity, ISystemContext ctx);

        void Delete(object entityId, ISystemContext ctx);
    }
}
