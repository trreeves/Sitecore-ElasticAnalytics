namespace ElasticAnalytics.Utils.DateTime
{
    public class StandardDateTimeController : IDateTimeController
    {
        public System.DateTime UtcNow
        {
            get
            {
                return System.DateTime.UtcNow;
            }
        }
    }
}
