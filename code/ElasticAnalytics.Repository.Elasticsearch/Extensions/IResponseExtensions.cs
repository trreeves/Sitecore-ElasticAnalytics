namespace ElasticAnalytics.Repository.Elasticsearch.Extensions
{
    using ElasticAnalytics.Repository.Elasticsearch.Exceptions;

    using Nest;

    public static class ResponseExtensions
    {
        public static bool DocumentAlreadyExists(this IResponse response)
        {
            return response.ServerError != null && response.ServerError.Status == 409;
        }

        public static bool InvalidConcurrency(this IResponse response)
        {
            return response.ServerError != null && response.ServerError.Status == 409;
        }

        public static void AssertSuccess(this IResponse response, string when)
        {
            if (!response.IsValid)
            {
                if (response.ServerError != null)
                {
                    throw new EsDataPersistanceException(when, response);
                }

                throw new EsDataPersistanceException("Unknown ES error - " + when);
            }
        }

        public static bool DocumentNotFound<T>(this IGetResponse<T> response) where T : class
        {
            return
                !response.Found && 
                (response.ServerError != null && string.IsNullOrWhiteSpace(response.ServerError.ExceptionType)) &&
                response.ConnectionStatus.HttpStatusCode == 404;
        }
    }
}