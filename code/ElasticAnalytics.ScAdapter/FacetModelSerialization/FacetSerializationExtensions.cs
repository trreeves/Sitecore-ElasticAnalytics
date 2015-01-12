namespace ElasticAnalytics.ScAdapter.FacetModelSerialization
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    using Newtonsoft.Json.Linq;

    using Sitecore.Analytics.Model.Framework;

    public static class FacetSerializationExtensions
    {
        public static void SerializeAsFacet<TRoot, TFacet>(
            this TRoot dataSourceRoot,
            Expression<Func<TRoot, TFacet>> facetDataSourcePointer,
            JObject target,
            IElementJsonConverter converter) where TFacet : class
        {
            var facetPropertyInfo = ExtractPropertyInfo(facetDataSourcePointer);
            var facetName = facetPropertyInfo.Name;
            var facet = facetDataSourcePointer.Compile()(dataSourceRoot);

            if (facet == null)
            {
                throw new ArgumentOutOfRangeException("facetDataSourcePointer", "Yields null");
            }

            var facetDataSource = facet as IFacet;
            if (facetDataSource == null)
            {
                throw new ArgumentOutOfRangeException("facetDataSourcePointer", "Does not specify an IFacet.");
            }

            target.Add(new JProperty(facetName, converter.Serialize(facetDataSource)));
        }

        public static void DeserializeAsFacet<TRoot, TFacet>(
            this TRoot root,
            Expression<Func<TRoot, TFacet>> facetPointer,
            JObject dataSource,
            IElementJsonConverter converter) where TFacet : class
        {
            var facetPropertyInfo = ExtractPropertyInfo(facetPointer);
            var facetName = facetPropertyInfo.Name;
            var facet = facetPointer.Compile()(root);

            if (facet == null)
            {
                throw new ArgumentOutOfRangeException("facetPointer", "Yields null");
            }

            var targetFacet = facet as IFacet;
            if (targetFacet == null)
            {
                throw new ArgumentOutOfRangeException("facetPointer", "Does not specify an IFacet.");
            }

            var facetTok = dataSource[facetName];
            if (facetTok == null)
            {
                return;
            }

            var facetObj = facetTok as JObject;
            if (facetObj == null)
            {
                return;
            }

            converter.Deserialize(facetObj, targetFacet);
        }

        public static void PopulateAsFacetMember<TFacet, TAttribute>(
            this TFacet facet,
            Expression<Func<TFacet, TAttribute>> propertyPointer,
            TAttribute dataSource)
        {
            var facetPropertyInfo = ExtractPropertyInfo(propertyPointer);

            var targetFacet = facet as IFacet;
            if (targetFacet == null)
            {
                throw new ArgumentOutOfRangeException("facet", "Does not specify an IFacet.");
            }

            var elementMember = targetFacet.Members[facetPropertyInfo.Name];
            var attributeMember = elementMember as IModelAttributeMember<TAttribute>;
            if (attributeMember == null)
            {
                throw new ArgumentOutOfRangeException("propertyPointer", "Does not specify a IModelAttributeMember.");
            }

            attributeMember.Value = dataSource;
        }

        private static PropertyInfo ExtractPropertyInfo<TSource, TProperty>(
            Expression<Func<TSource, TProperty>> propertyLambda)
        {
            var type = typeof(TSource);

            var member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(
                    string.Format("Expression '{0}' refers to a method, not a property.", propertyLambda.ToString()));

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(
                    string.Format("Expression '{0}' refers to a field, not a property.", propertyLambda.ToString()));

            if (type != propInfo.ReflectedType && !propInfo.ReflectedType.IsAssignableFrom(type))
                throw new ArgumentException(
                    string.Format(
                        "Expresion '{0}' refers to a property that is not from type {1}.",
                        propertyLambda.ToString(),
                        type));

            return propInfo;
        }
    }
}