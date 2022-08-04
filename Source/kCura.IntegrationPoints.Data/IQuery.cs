namespace kCura.IntegrationPoints.Data
{
    public interface IQuery<out T>
    {
        T Execute();
    }
}
