using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria;

public class RollBackNetPositionCommand : NonQueryCommand
{
    public string CricketIds { get; set; }

    public string Action { get; set; }

    public int AdminId { get; set; }

    public string Reason { get; set; }

    public override string GetSettingKey() => "NAP_RollbackCricketList";

    public override bool IsValid() => true;
}