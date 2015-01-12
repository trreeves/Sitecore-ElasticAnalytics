namespace ElasticAnalytics.Service.Types
{
    public interface ISystemContext
    {
        string InstanceKey { get; }

        string SystemPrefix { get; }
    }
}
