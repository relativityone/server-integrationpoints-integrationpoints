namespace kCura.IntegrationPoints.Web.SignalRHubs
{
    public class IntegrationPointDataHubKey
    {
        public IntegrationPointDataHubKey(int workspaceId, int integrationPointId, int userId)
        {
            WorkspaceId = workspaceId;
            IntegrationPointId = integrationPointId;
            UserId = userId;
        }

        public int WorkspaceId { get; }
        public int IntegrationPointId { get; }
        public int UserId { get; }

        protected bool Equals(IntegrationPointDataHubKey other)
        {
            return WorkspaceId == other.WorkspaceId && IntegrationPointId == other.IntegrationPointId && UserId == other.UserId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((IntegrationPointDataHubKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = WorkspaceId;
                hashCode = (hashCode * 397) ^ IntegrationPointId;
                hashCode = (hashCode * 397) ^ UserId;
                return hashCode;
            }
        }

        public static bool operator ==(IntegrationPointDataHubKey left, IntegrationPointDataHubKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(IntegrationPointDataHubKey left, IntegrationPointDataHubKey right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return $"{nameof(WorkspaceId)}:{WorkspaceId};{nameof(IntegrationPointId)}:{IntegrationPointId};{nameof(UserId)}:{UserId}";
        }
    }
}