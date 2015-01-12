namespace ElasticAnalytics.ScAdapter.FacetModelSerialization
{
    using Newtonsoft.Json.Linq;

    using Sitecore.Analytics.Model.Framework;

    public interface IElementJsonConverter
    {
        JObject Serialize(IElement dataSource);

        void Deserialize(JObject dataSource, IElement destination);
    }
}