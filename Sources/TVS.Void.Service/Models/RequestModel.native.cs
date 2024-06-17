using System;
using System.Collections.Generic;

namespace TVS.Void.Service.Models;

public class RequestModel
{
    public int RequestId { get; set; }
    public string ActionName { get; set; }
    public int VoidActionId { get; set; }
    public int IssueTypeId { get; set; }
    public int MatchId { get; set; }
}

public class RequestDetail
{
    public int RequestId { get; set; }
    public int MatchId { get; set; }
}

public class RequestDetailTicketModel
{
    public int RequestId { get; set; }
    public int IssueTypeId { get; set; }
    public int SiteDbGroupId { get; set; }
    public long TransId { get; set; }
    public int MatchId { get; set; }
    public int MainTransId { get; set; }
    public DateTime TransDate { get; set; }
    public DateTime WinLostDate { get; set; }
    public int StatusId { get; set; }
    public int VoidActionId { get; set; }
    public int VoidingTypeId { get; set; }
    public int ValidationType { get; set; }
}

public class RequestDetailTicketModelEqualityComparer : IEqualityComparer<RequestDetailTicketModel>
{
    public bool Equals(RequestDetailTicketModel x, RequestDetailTicketModel y)
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
        return x.RequestId == y.RequestId && x.IssueTypeId == y.IssueTypeId && x.SiteDbGroupId == y.SiteDbGroupId
               && x.TransId == y.TransId && x.MatchId == y.MatchId && x.MainTransId == y.MainTransId;
    }

    public int GetHashCode(RequestDetailTicketModel obj)
    {
        return HashCode.Combine(obj.RequestId, obj.IssueTypeId, obj.SiteDbGroupId, obj.TransId, obj.MatchId, obj.MainTransId);
    }
}