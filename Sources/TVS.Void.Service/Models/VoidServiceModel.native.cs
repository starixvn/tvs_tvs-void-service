using System;
using System.Collections.Generic;

namespace TVS.Void.Service.Models
{
    public class VoidServiceModel
    {
        public string BroadcastSessionId { get; set; }
        public int SiteDbGroupId { get; set; }
        public int RequestId { get; set; }
        public int VoidingHistoryId { get; set; }
        public int StatusId { get; set; }
        public bool IsAutoVoid { get; set; }
        public int IssueTypeId { get; set; }
        public int MatchId { get; set; }
        public int CreatedTimeDuration { get; set; }
    }

    public class VoidServiceModelEqualityComparer : IEqualityComparer<VoidServiceModel>
    {
        public bool Equals(VoidServiceModel x, VoidServiceModel y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }
            return x.BroadcastSessionId == y.BroadcastSessionId && x.SiteDbGroupId == y.SiteDbGroupId && x.RequestId == y.RequestId
                   && x.VoidingHistoryId == y.VoidingHistoryId && x.StatusId == y.StatusId && x.IsAutoVoid == y.IsAutoVoid
                   && x.IssueTypeId == y.IssueTypeId && x.MatchId == y.MatchId;
        }

        public int GetHashCode(VoidServiceModel obj)
        {
            return HashCode.Combine(obj.BroadcastSessionId, obj.SiteDbGroupId, obj.RequestId, obj.VoidingHistoryId, obj.StatusId, obj.IsAutoVoid, obj.IssueTypeId, obj.MatchId);
        }
    }
}