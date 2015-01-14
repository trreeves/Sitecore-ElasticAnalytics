namespace ElasticAnalytics.Repository.Elasticsearch.Repositories
{
    using ElasticAnalytics.Repository.Elasticsearch.Client;
    using ElasticAnalytics.Repository.Elasticsearch.Extensions;
    using ElasticAnalytics.Repository.Elasticsearch.PersistenceModel;
    using ElasticAnalytics.Repository.Elasticsearch.QueryContext;
    using ElasticAnalytics.Repository.Types;
    using ElasticAnalytics.Repository.Types.Repositories;
    using ElasticAnalytics.Service.Types;
    using ElasticAnalytics.Utils.AutoMapper;

    public class EsConcurrencyControlRepository<T, TRepo> : IConcurrencyControlRepository<T>
        where T : IVersionable
        where TRepo : class, IEsDoc
    {
        protected readonly IEsClient client;

        protected readonly ITypeMapper repoModelMapper;

        protected readonly IRequestConfiguration requestConfig;

        protected readonly string loggingName;

        public EsConcurrencyControlRepository(
            IEsClient client,
            ITypeMapper repoMapper,
            IRequestConfiguration requestConfig)
        {
            this.client = client;
            this.repoModelMapper = repoMapper;
            this.requestConfig = requestConfig;
            this.loggingName = typeof(TRepo).Name;
        }

        public virtual bool Create(T entity, ISystemContext ctx)
        {
            var repoDoc = this.repoModelMapper.Map<T, TRepo>(entity);

            var res = this.client.Create(
                new EsQueryContext(
                    this.requestConfig.GenerateIndexIdentifier(ctx),
                    this.requestConfig.GenerateTypeIdentifier(ctx)),
                repoDoc, 
                ctx);

            if (res.DocumentAlreadyExists())
            {
                return false;
            }

            res.AssertSuccess("Creating " + this.loggingName);

            entity.SetVersion(long.Parse(res.Version));

            return res.Created;
        }

        public virtual T Get(object entityId, ISystemContext ctx)
        {
            var res = this.client.Get<TRepo>(
                new EsQueryContext(
                    this.requestConfig.GenerateIndexIdentifier(ctx),
                    this.requestConfig.GenerateTypeIdentifier(ctx)),
                entityId.ToString(), 
                ctx);

            if (res.DocumentNotFound())
            {
                return default(T);
            }

            res.AssertSuccess("Getting " + this.loggingName);

            var entity = this.repoModelMapper.Map<TRepo, T>(res.Source);
            entity.SetVersion(long.Parse(res.Version));
            return entity;
        }

        public virtual void Save(T entity, ISystemContext ctx)
        {
            var repoDoc = this.repoModelMapper.Map<T, TRepo>(entity);

            var res = this.client.Save(
                new EsQueryContext(
                    this.requestConfig.GenerateIndexIdentifier(ctx),
                    this.requestConfig.GenerateTypeIdentifier(ctx)),
                repoDoc, 
                ctx);

            res.AssertSuccess("Saving " + this.loggingName);
            entity.SetVersion(long.Parse(res.Version));
        }

        public virtual bool SaveOptimistically(T entity, ISystemContext ctx)
        {
            var repoEntity = this.repoModelMapper.Map<T, TRepo>(entity);

            var res = this.client.Save(
                new EsQueryContext(
                    this.requestConfig.GenerateIndexIdentifier(ctx),
                    this.requestConfig.GenerateTypeIdentifier(ctx)),
                repoEntity, 
                ctx, 
                entity.Version);

            if (res.InvalidConcurrency())
            {
                return false;
            }

            res.AssertSuccess("Saving optimistically " + this.loggingName);
            entity.SetVersion(long.Parse(res.Version));
            return true;
        }

        public virtual void Delete(object entityId, ISystemContext ctx)
        {
            var res = this.client.Delete<TRepo>(
               new EsQueryContext(
                    this.requestConfig.GenerateIndexIdentifier(ctx),
                    this.requestConfig.GenerateTypeIdentifier(ctx)),
                entityId.ToString(), 
                ctx);

            res.AssertSuccess("Deleting " + this.loggingName);
        }

        public virtual bool DeleteOptimistically(object entityId, long version, ISystemContext ctx)
        {
            var res = this.client.Delete<TRepo>(
               new EsQueryContext(
                    this.requestConfig.GenerateIndexIdentifier(ctx),
                    this.requestConfig.GenerateTypeIdentifier(ctx)),
                entityId.ToString(), 
                ctx, 
                version);

            if (res.InvalidConcurrency())
            {
                return false;
            }

            res.AssertSuccess("Deleting Optimistically " + this.loggingName);

            return true;
        }
    }
}