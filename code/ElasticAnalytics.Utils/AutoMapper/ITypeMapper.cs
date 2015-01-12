namespace ElasticAnalytics.Utils.AutoMapper
{
    public interface ITypeMapper
    {
        TDest Map<TSource, TDest>(TSource sourceObj);

        TDest Map<TSource, TDest>(TSource sourceObj, TDest destObj);
    }
}