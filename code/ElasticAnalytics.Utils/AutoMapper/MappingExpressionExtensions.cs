namespace ElasticAnalytics.Utils.AutoMapper
{
    using System;

    using global::AutoMapper;

    public static class MappingExpressionExtensions
    {
        public static IMappingExpression<TSource, TDestination> ConstructUsing<TSource, TDestination>(
            this IMappingExpression<TSource, TDestination> expr,
            Func<ResolutionContext, TSource, TDestination> ctor)
        {
            return expr.ConstructUsing(
                ctx =>
                {
                    var typedSourceVal = (TSource)ctx.SourceValue;
                    return ctor(ctx, typedSourceVal);
                });
        }

        public static IMappingExpression<TSource, TDestination> ConstructUsing<TSource, TDestination>(
            this IMappingExpression<TSource, TDestination> expr,
            Func<ResolutionContext, TSource, TDestination, TDestination> ctor)
        {
            return expr.ConstructUsing(
                ctx =>
                {
                    var typedSourceVal = (TSource)ctx.SourceValue;
                    var typedTargetVal = (TDestination)ctx.DestinationValue;
                    return ctor(ctx, typedSourceVal, typedTargetVal);
                });
        }
    }
}
