using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria
{
    public class UpdateVoidServiceCriteria : NonQueryCommand
    {
        public int SiteDbGroupId { get; set; }
        public int RequestId { get; set; }
        public int VoidingHistoryId { get; set; }

        public int StatusId { get; set; }

        public override string GetSettingKey() => "TicketVoiding_Void_Service_Update";

        public override bool IsValid() => true;
    }
}