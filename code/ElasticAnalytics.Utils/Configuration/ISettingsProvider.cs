namespace ElasticAnalytics.SessionRepository.Types.Configuration
{
    public interface ISettingsProvider
    {
        string Value(string key);
    }
}
