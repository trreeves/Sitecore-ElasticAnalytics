namespace ElasticAnalytics.ScAdapter.FacetModelSerialization
{
    using Newtonsoft.Json.Linq;

    using Sitecore.Analytics.Model.Framework;

    public class ElementModelSerializer : IModelMemberSerializer
    {
        public bool CanProcess(IModelMember member)
        {
            return member is IModelElementMember;
        }

        public JToken Serialize(
            IModelMember dataSource, 
            ISerializerProvider provider, 
            IElementSerializer elementSerializer)
        {
            var elDataSource = dataSource as IModelElementMember;

            var res = new JProperty(elDataSource.Name);
            res.Value = elementSerializer.Serialize(elDataSource.Element, provider);
            return res;
        }

        public void Deserialize(
            JToken dataSource,
            IModelMember destination,
            ISerializerProvider provider,
            IElementSerializer elementSerializer)
        {
            var current = destination as IModelElementMember;

            var jObjDataSource = dataSource as JObject;
            if (jObjDataSource == null) return;

            elementSerializer.Deserialize(jObjDataSource, current.Element, provider);
        }
    }
}