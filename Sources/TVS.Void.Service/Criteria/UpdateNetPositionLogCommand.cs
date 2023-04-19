using System;
using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria;

public class UpdateNetPositionLogCommand : NonQueryCommand
{
    public int ServiceType { get; set; }

    public string TransIds { get; set; }

    public string Action { get; set; }

    public int ServiceStatus { get; set; }

    public int AdminId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Reason { get; set; }

    public override string GetSettingKey() => "Update_Net_Position_Log";

    public override bool IsValid() => true;
}