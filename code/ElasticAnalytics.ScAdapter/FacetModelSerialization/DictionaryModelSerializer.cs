namespace ElasticAnalytics.ScAdapter.FacetModelSerialization
{
    using System.Linq;

    using Newtonsoft.Json.Linq;

    using Sitecore.Analytics.Model.Framework;

    public class DictionaryModelSerializer : IModelMemberSerializer
    {
        public bool CanProcess(IModelMember member)
        {
            return member is IModelDictionaryMember;
        }

        public JToken Serialize(
            IModelMember dataSource, 
            ISerializerProvider provider, 
            IElementSerializer elementSerializer)
        {
            var dictDataSource = dataSource as IModelDictionaryMember;

            var dict = new JObject();
            var res = new JProperty(dictDataSource.Name, dict);
            foreach (var key in dictDataSource.Elements.Keys)
            {
                var newEl = new JProperty(key);
                newEl.Value = elementSerializer.Serialize(dictDataSource.Elements[key], provider);
                dict.Add(newEl);
            }

            return res;
        }

        public void Deserialize(
            JToken dataSource,
            IModelMember destination,
            ISerializerProvider provider,
            IElementSerializer elementSerializer)
        {
            var current = destination as IModelDictionaryMember;

            var jObjDataSource = dataSource as JObject;
            if (jObjDataSource == null) return;

            foreach (var childJProp in jObjDataSource.Children().OfType<JProperty>())
            {
                var childJObj = childJProp.Value as JObject;
                if (childJObj == null) return;

                var newEl = current.Elements.Create(childJProp.Name);
                elementSerializer.Deserialize(childJObj, newEl, provider);
            }
        }
    }
}