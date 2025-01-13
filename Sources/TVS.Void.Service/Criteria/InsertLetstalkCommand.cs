using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria;

public class InsertLetstalkCommand : NonQueryCommand
{
    public int RequestId { get; set; }

    public string BroadcastSessionId { get; set; }

    public int StatusId { get; set; }

    public override string GetSettingKey() => "TicketVoiding_Letstalk_Insert";

    public override bool IsValid() => true;
}