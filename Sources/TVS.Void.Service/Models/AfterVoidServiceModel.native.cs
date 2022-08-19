using System;
using System.Collections.Generic;

namespace TVS.Void.Service.Models
{
    public class AfterVoidServiceModel
    {
        public int SessionId { get; set; }
        public int MatchId { get; set; }
        public int RequestId { get; set; }
        public int VoidingHistoryId { get; set; }
        public int StatusId { get; set; }
        public string NegativeBalanceUsers { get; set; }
    }

    public class AfterVoidServiceModelEqualComparer : IEqualityComparer<AfterVoidServiceModel>
    {
        public bool Equals(AfterVoidServiceModel x, AfterVoidServiceModel y)
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
            return x.SessionId == y.SessionId && x.MatchId == y.MatchId && x.RequestId == y.RequestId &&
                   x.VoidingHistoryId == y.VoidingHistoryId && x.StatusId == y.StatusId && x.NegativeBalanceUsers == y.NegativeBalanceUsers;
        }

        public int GetHashCode(AfterVoidServiceModel obj)
        {
            return HashCode.Combine(obj.SessionId, obj.MatchId, obj.RequestId, obj.VoidingHistoryId, obj.StatusId, obj.NegativeBalanceUsers);
        }
    }
}