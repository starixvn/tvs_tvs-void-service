using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest;

public interface UpdateIsProcessedStage
{
    Task Execute(IEnumerable<RequestDetailTicketModel> tickets, IList<string> logData);
}

public class UpdateIsProcessedStageImp : UpdateIsProcessedStage
{
    private readonly RequestTicketService requestTicketService;

    public UpdateIsProcessedStageImp(RequestTicketService requestTicketService)
    {
        this.requestTicketService = requestTicketService;
    }

    public async Task Execute(IEnumerable<RequestDetailTicketModel> tickets, IList<string> logData)
    {
        if (tickets.Any())
        {
            await requestTicketService.UpdateIsProcessed(tickets, Enumerable.Empty<RequestDetailTicketModel>(), Enumerable.Empty<RequestDetailTicketModel>());
            logData.Add($"{nameof(RequestTicketService.UpdateIsProcessed)}-LoseTickets-OK");
        }
    }
}