namespace ElasticAnalytics.Repository.Elasticsearch.Client
{
    using System;

    using ElasticAnalytics.Repository.Elasticsearch.PersistenceModel;
    using ElasticAnalytics.Repository.Elasticsearch.QueryContext;
    using ElasticAnalytics.Service.Types;

    using global::Elasticsearch.Net;

    using Nest;

    public class EsClient : IEsClient
    {
        protected readonly ElasticClient client;

        public EsClient(ElasticClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            this.client = client;
        }

        public IIndexResponse Create<T>(IQueryContext qCtx, T doc, ISystemContext ctx) 
            where T : class, IEsDoc
        {
            return this.client.Index(doc, 
                q => q.Index(qCtx.Index)
                     .Type(qCtx.Type)
                     .Id(doc._id)
                     .Consistency(Consistency.Quorum)
                     .Replication(Replication.Async)
                     .OpType(OpType.Create)
                     .Routing(qCtx.ShardKey?? doc._id));
        }

        public IGetResponse<T> Get<T>(IQueryContext qCtx, string id, ISystemContext ctx) 
            where T : class, IEsDoc
        {
            return this.client.Get<T>(q =>
                    q.Index(qCtx.Index)
                    .Type(qCtx.Type)
                    .Id(id)
                    .Routing(qCtx.ShardKey??id));
        }

        public IIndexResponse Save<T>(IQueryContext qCtx, T doc, ISystemContext ctx, long version = -1) 
            where T : class, IEsDoc
        {
            var query = new IndexDescriptor<T>();
            query
                 .Index(qCtx.Index)
                 .Type(qCtx.Type)
                 .Id(doc._id)
                 .Consistency(Consistency.Quorum)
                 .Replication(Replication.Async)
                 .Routing(qCtx.ShardKey??doc._id);

            if (version > 0)
            {
                query.Version(version); // will ensure this request fails if doc has changed since
            }

            return this.client.Index(doc, l => query);
        }

        public IDeleteResponse Delete<T>(IQueryContext qCtx, string id, ISystemContext ctx, long version = -1)
        {
            var query = new DeleteDescriptor<string>();
            query
                 .Index(qCtx.Index)
                 .Type(qCtx.Type)
                 .Id(id)
                 .Consistency(Consistency.Quorum)
                 .Replication(Replication.Async)
                 .Routing(qCtx.ShardKey??id);

            if (version > 0)
            {
                query.Version(version); // will ensure this request fails if doc has changed since
            }

            return this.client.Delete(id, q => query);
        }
    }
}
