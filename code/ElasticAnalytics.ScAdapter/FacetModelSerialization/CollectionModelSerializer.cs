namespace ElasticAnalytics.ScAdapter.FacetModelSerialization
{
    using System.Linq;

    using Newtonsoft.Json.Linq;

    using Sitecore.Analytics.Model.Framework;

    public class CollectionModelSerializer : IModelMemberSerializer
    {
        public bool CanProcess(IModelMember member)
        {
            return member is IModelCollectionMember;
        }

        public JToken Serialize(
            IModelMember dataSource, 
            ISerializerProvider provider, 
            IElementSerializer elementSerializer)
        {
            var colDataSource = dataSource as IModelCollectionMember;

            var arr = new JArray();
            var res = new JProperty(colDataSource.Name, arr);
            foreach (var element in colDataSource.Elements)
            {
                var newEl = elementSerializer.Serialize(element, provider);
                arr.Add(newEl);
            }

            return res;
        }

        public void Deserialize(
            JToken dataSource,
            IModelMember destination,
            ISerializerProvider provider,
            IElementSerializer elementSerializer)
        {
            var current = destination as IModelCollectionMember;

            var jArrayDataSource = dataSource as JArray;
            if (jArrayDataSource == null) return;

            foreach (var childJObject in jArrayDataSource.Children().OfType<JObject>())
            {
                var newEl = current.Elements.Create();
                elementSerializer.Deserialize(childJObject, newEl, provider);
            }
        }
    }
}