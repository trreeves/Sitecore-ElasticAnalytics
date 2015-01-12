namespace ElasticAnalytics.ScAdapter.FacetModelSerialization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json.Linq;

    using Sitecore.Analytics.Model.Framework;

    public class ElementSerializer : ISerializerProvider, IElementSerializer, IElementJsonConverter
    {
        protected readonly ISet<IModelMemberSerializer> serializers;

        public ElementSerializer(params IModelMemberSerializer[] serializers)
        {
            this.serializers = new HashSet<IModelMemberSerializer>(serializers);
        }

        public void Deserialize(JObject dataSource, IElement destination)
        {
            this.Deserialize(dataSource, destination, this);
        }

        public void Deserialize(JObject dataSource, IElement destination, ISerializerProvider provider)
        {
            foreach (var member in destination.Members)
            {
                var currentMemberSource = dataSource[member.Name];
                if (currentMemberSource == null)
                {
                    continue;
                }

                this.GetSerializer(member).Deserialize(currentMemberSource, member, provider, this);
            }
        }

        public JObject Serialize(IElement dataSource)
        {
            return this.Serialize(dataSource, this);
        }

        public JObject Serialize(IElement dataSource, ISerializerProvider provider)
        {
            var res = new JObject();

            foreach (var member in dataSource.Members)
            {
                res.Add(this.GetSerializer(member).Serialize(member, this, this));
            }

            return res;
        }

        public IModelMemberSerializer GetSerializer(IModelMember modelMember)
        {
            var serializer = this.serializers.FirstOrDefault(s => s.CanProcess(modelMember));
            if (serializer == null)
            {
                throw new InvalidOperationException("No model member serializer has been registered for a " + modelMember.GetType().Name);
            }

            return serializer;
        }
    }
}