namespace ElasticAnalytics.Model.Locking
{
    public enum LeaseOwnerType
    {
        /// <summary>
        /// Lock is obtained in the context of an HTTP request.
        /// </summary>
        WebCluster,

        /// <summary>
        /// Lease owner is code that resides outside of the context of an HTTP request.
        /// </summary>
        OutOfRequestWorker
    }
}
