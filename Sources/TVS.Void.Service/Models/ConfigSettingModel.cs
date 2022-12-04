namespace TVS.Void.Service.Models
{
    public class ConfigSettingModel
    {
        public bool IsScanningAutoVoidRequestJobExecuting { get; set; }
        public bool IsVoidJobExecuting { get; set; }
        public bool IsCheckingVoidStatusJobExecuting { get; set; }
        public bool IsCheckTicketStatusJobExecuting { get; set; }
    }
}