namespace ElasticAnalytics.Repository.Types
{
    using ElasticAnalytics.Service.Types;

    public interface IRequestConfiguration
    {
        string GenerateIndexIdentifier(ISystemContext ctx);

        string GenerateTypeIdentifier(ISystemContext ctx);
    }
}