using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria;

public class UpdateIsProcessedForRequestTicketsCriteria : NonQueryCommand
{
    public string SingleTicketsJson { get; set; }
    public string ParlayTicketsJson { get; set; }
    public string CricketTicketsJson { get; set; }
    public string CancelCashOutTicketsJson { get; set; }  //Just use in manual void

    public override string GetSettingKey() => "TicketVoiding_TicketProcess_Update";

    public override bool IsValid() => true;
}