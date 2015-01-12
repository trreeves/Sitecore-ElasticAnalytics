namespace ElasticAnalytics.ScAdapter.FacetModelSerialization
{
    using Sitecore.Analytics.Model.Framework;

    public interface ISerializerProvider
    {
        IModelMemberSerializer GetSerializer(IModelMember modelMember);
    }
}