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

    public class EsRepository<T, TRepo> : IRepository<T>
        where TRepo : class, IEsDoc
    {
        protected readonly IEsClient client;

        protected readonly ITypeMapper repoModelMapper;

        protected readonly IRequestConfiguration requestConfig;

        protected readonly string loggingName;

        public EsRepository(
            IEsClient client,
            ITypeMapper repoModelMapper,
            EsRequestConfiguration<T, TRepo> requestConfig)
        {
            this.client = client;
            this.repoModelMapper = repoModelMapper;
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

            return this.repoModelMapper.Map<TRepo, T>(res.Source);
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

            res.AssertSuccess("saving " + this.loggingName);
        }

        public virtual void Delete(object entityId, ISystemContext ctx)
        {
            var res = this.client.Delete<TRepo>(
                new EsQueryContext(
                    this.requestConfig.GenerateIndexIdentifier(ctx),
                    this.requestConfig.GenerateTypeIdentifier(ctx)),
                entityId.ToString(), 
                ctx);

            res.AssertSuccess("deleting " + this.loggingName);
        }
    }
}
