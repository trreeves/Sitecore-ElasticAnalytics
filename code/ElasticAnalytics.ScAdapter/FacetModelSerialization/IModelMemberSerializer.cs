namespace ElasticAnalytics.ScAdapter.FacetModelSerialization
{
    using Newtonsoft.Json.Linq;

    using Sitecore.Analytics.Model.Framework;

    public interface IModelMemberSerializer
    {
        bool CanProcess(IModelMember member);

        JToken Serialize(IModelMember dataSource, ISerializerProvider provider, IElementSerializer elementSerializer);

        void Deserialize(
            JToken dataSource,
            IModelMember destination,
            ISerializerProvider provider,
            IElementSerializer elementSerializer);
    }
}