namespace ElasticAnalytics.ScAdapter.FacetModelSerialization
{
    using Newtonsoft.Json.Linq;

    using Sitecore.Analytics.Model.Framework;

    public class AttributeModelSerializer : IModelMemberSerializer
    {
        public bool CanProcess(IModelMember member)
        {
            return member is IModelAttributeMember;
        }

        public JToken Serialize(
            IModelMember dataSource,
            ISerializerProvider provider,
            IElementSerializer elementSerializer)
        {
            var attrDataSource = dataSource as IModelAttributeMember;
            return new JProperty(attrDataSource.Name, attrDataSource.Value);
        }

        public void Deserialize(
            JToken dataSource,
            IModelMember destination,
            ISerializerProvider provider,
            IElementSerializer elementSerializer)
        {
            var jValue = dataSource as JValue;
            if (jValue == null) return;

            var current = destination as IModelAttributeMember;

            current.Value = jValue.ToObject(current.Type);
        }
    }
}