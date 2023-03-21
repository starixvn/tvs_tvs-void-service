using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest;

public interface GetTicketsStage
{
    Task<(bool IsContinueFlow, GetTicketsStageOutput OutputData)> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IList<string> logData);
}

public class GetTicketsStageImp : GetTicketsStage
{
    private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
    private readonly RequestTicketService requestTicketService;

    public GetTicketsStageImp(AutoVoidRequestQueueService autoVoidRequestQueueService, RequestTicketService requestTicketService)
    {
        this.autoVoidRequestQueueService = autoVoidRequestQueueService;
        this.requestTicketService = requestTicketService;
    }

    public async Task<(bool, GetTicketsStageOutput)> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IList<string> logData)
    {
        var (singleTickets, parlayTickets, cricketTickets)
            = await requestTicketService.GetTicketsByRequestsAndSiteDbGroupId(siteDbGroupId, queue.RequestIds);
        logData.Add($"{nameof(GetTicketsStage)} - SingleCount: {singleTickets.Count()} - ParlayCount: {parlayTickets.Count()}");

        if (!singleTickets.Any() && !parlayTickets.Any())
        {
            await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, "No ticket found.");
            return (false, null);
        }

        return (true, new GetTicketsStageOutput
        {
            SingleTickets = singleTickets,
            ParlayTickets = parlayTickets,
            CricketTickets = cricketTickets,
        });
    }
}

public class GetTicketsStageOutput
{
    public IEnumerable<RequestDetailTicketModel> SingleTickets { get; set; }
    public IEnumerable<RequestDetailTicketModel> ParlayTickets { get; set; }
    public IEnumerable<RequestDetailTicketModel> CricketTickets { get; set; }
}