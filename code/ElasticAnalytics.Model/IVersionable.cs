public interface IVersionable
{
    long Version { get; }

    void SetVersion(long newVersion);

    void IncrementVersion(long increment = 1);
}