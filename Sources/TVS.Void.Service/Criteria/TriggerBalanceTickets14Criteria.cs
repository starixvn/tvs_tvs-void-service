using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria;

public class TriggerBalanceTickets14Criteria : NonQueryCommand
{
    public string BeforeTransInfoJson { get; set; }

    public string AfterTransInfoJson { get; set; }

    public override string GetSettingKey() => "TicketVoiding_Admin_Bet14_BalanceTrigger_Insert";

    public override bool IsValid() => true;
}

public class TriggerBalanceTickets14BackupCriteria : NonQueryCommand
{
    public string BeforeTransInfoJson { get; set; }

    public string AfterTransInfoJson { get; set; }

    public override string GetSettingKey() => "TicketVoiding_Admin_Bet14_BalanceTrigger_Insert_Backup";

    public override bool IsValid() => true;
}