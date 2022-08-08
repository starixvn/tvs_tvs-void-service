using System;

namespace TVS.Void.Service.Models
{
    public class RequestDetailModel : IEquatable<RequestDetailModel>
    {
        public int RequestId { get; set; }
        public int IssueTypeId { get; set; }

        public bool Equals(RequestDetailModel other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return RequestId == other.RequestId && IssueTypeId == other.IssueTypeId;
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

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((RequestDetailModel)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(RequestId, IssueTypeId);
        }
    }

    public class RequestDetailTicketModel
    {
        public int RequestId { get; set; }
        public int IssueTypeId { get; set; }
        public int SiteDbGroupId { get; set; }
        public int TransId { get; set; }
        public int MatchId { get; set; }
        public int MainTransId { get; set; }
    }
}