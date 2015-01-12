namespace ElasticAnalytics.Repository.Elasticsearch.Exceptions
{
    using ElasticAnalytics.Repository.Types.Exceptions;

    using global::Elasticsearch.Net;

    using Nest;

    public class EsDataPersistanceException : ElasticDataPersistenceException
    {
        protected readonly ElasticsearchServerError error;

        protected readonly IElasticsearchResponse requestInfo;

        public EsDataPersistanceException(string message) : base(message)
        {
            this.error = null;
        }

        public EsDataPersistanceException(string action, IResponse response) :
            base(string.Format(
                    "Elasticsearch error occured when {0} : \n\n{1}", 
                    action, 
                    string.Format(
                        "({0}) {1} - {2}\n---------------------------------------------\n{3}", 
                        response.RequestInformation.HttpStatusCode,
                        response.RequestInformation.RequestMethod,
                        response.RequestInformation.RequestUrl,
                        response.RequestInformation.ResponseRaw == null ?
                            string.Empty :
                            System.Text.Encoding.Default.GetString(response.RequestInformation.ResponseRaw))))
        {
            this.error = response.ServerError;
            this.requestInfo = response.RequestInformation;
        }
    }
}