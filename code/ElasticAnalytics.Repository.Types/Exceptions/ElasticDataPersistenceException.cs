using System;

namespace ElasticAnalytics.Repository.Types.Exceptions
{
    using System.Runtime.Serialization;

    public class ElasticDataPersistenceException : Exception
    {
        public ElasticDataPersistenceException(string message) : 
            base(message)
        {
        }

        public ElasticDataPersistenceException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

        protected ElasticDataPersistenceException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }
    }
}
