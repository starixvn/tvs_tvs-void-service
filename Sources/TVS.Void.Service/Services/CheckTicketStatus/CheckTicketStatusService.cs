using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;

namespace TVS.Void.Service.Services.CheckTicketStatus
{
    public interface CheckTicketStatusService
    {
        Task Execute();
    }

    public class CheckTicketStatusServiceImp : CheckTicketStatusService
    {
        private readonly RequestTicketService requestTicketService;
        private readonly InsertAutoVoidQueueStage insertAutoVoidQueueStage;
        private readonly LoggingService loggingService;

        public CheckTicketStatusServiceImp(RequestTicketService requestTicketService, InsertAutoVoidQueueStage insertAutoVoidQueueStage, LoggingService loggingService)
        {
            this.requestTicketService = requestTicketService;
            this.insertAutoVoidQueueStage = insertAutoVoidQueueStage;
            this.loggingService = loggingService;
        }

        public async Task Execute()
        {
            var logData = new List<string>();
            try
            {
                await ExecuteNormalFlow(logData);
            }
            catch (Exception e)
            {
                await HandleException(logData, e);
            }
        }

        private async Task ExecuteNormalFlow(IList<string> logData)
        {
            logData.Add(nameof(CheckTicketStatusService));
            var (singleTickets, parlayTickets) = await requestTicketService.GetTicketInAllRequests();

            foreach (var server in SiteDbGroupConstant.ServerList)
            {
                var singleTicketsInServer = singleTickets.Where(x => x.SiteDbGroupId == server.Id);
                var parlayTicketsInServer = parlayTickets.Where(x => x.SiteDbGroupId == server.Id);

                var requestsList = await requestTicketService.CheckStatusTicketForInsertAutoVoidQueue(server.Id, singleTicketsInServer, parlayTicketsInServer);
                await insertAutoVoidQueueStage.Execute(server.Id, requestsList, logData);
            }

            if (singleTickets.Any() || parlayTickets.Any())
            {
                logData.Add($"ServerTime: {DateTime.Now:MM/dd/yyyy HH:mm:ss.fff}");
                await loggingService.InfoJsonAsync(logData);
            }
        }

        private async Task HandleException(IList<string> logData, Exception exception)
        {
            await loggingService.InfoJsonAsync(logData);
            await loggingService.ErrorAsync($"{nameof(CheckTicketStatusService)} Error", exception);
        }
    }
}