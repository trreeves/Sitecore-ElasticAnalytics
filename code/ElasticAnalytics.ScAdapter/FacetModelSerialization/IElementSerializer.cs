namespace ElasticAnalytics.ScAdapter.FacetModelSerialization
{
    using Newtonsoft.Json.Linq;

    using Sitecore.Analytics.Model.Framework;

    public interface IElementSerializer
    {
        JObject Serialize(IElement dataSource, ISerializerProvider provider);

        void Deserialize(JObject dataSource, IElement destination, ISerializerProvider provider);
    }
}