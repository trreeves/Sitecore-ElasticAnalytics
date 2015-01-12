namespace ElasticAnalytics.Repository.Types
{
    using ElasticAnalytics.Service.Types;

    public interface IEsRequestConfiguration
    {
        string GenerateIndexIdentifier(ISystemContext ctx);

        string GenerateTypeIdentifier(ISystemContext ctx);
    }
}