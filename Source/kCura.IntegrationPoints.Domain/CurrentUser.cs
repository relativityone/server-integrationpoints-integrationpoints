namespace kCura.IntegrationPoints.Domain
{
    public class CurrentUser
    {
        public int ID { get; }

        public CurrentUser(int userID)
        {
            ID = userID;
        }
    }
}
