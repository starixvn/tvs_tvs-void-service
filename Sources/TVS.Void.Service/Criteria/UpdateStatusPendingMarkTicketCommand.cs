using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria;

public class UpdateStatusPendingMarkTicketCommand : NonQueryCommand
{
    public int StatusId { get; set; }

    public string TransIds { get; set; }

    public override string GetSettingKey() => "TicketVoiding_PendingMarkTickets_Update";

    public override bool IsValid() => true;
}