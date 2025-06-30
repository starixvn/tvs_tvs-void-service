using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Models;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.AutoMarkTicket;

public interface AutoMarkTicketService
{
    Task Execute();
}

public class AutoMarkTicketServiceImp(IDynamicRepository repository) : AutoMarkTicketService
{
    public async Task Execute()
    {
        var tickets = await repository.FetchAsync<AutoMarkTicketModel>(new SelectAutoMarkTicketCriteria());
        var groupTickets = tickets.GroupBy(t => t.SiteDbGroupId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var group in groupTickets)
        {
            var markResult = await MarkTicketBySiteDb(group.Key, group.Value);
            var status = markResult.IsSuccessful ? ExecuteStatus.Done : ExecuteStatus.Failed;
            await UpdateStatusTicket(status, group.Value);
        }
    }

    private async Task<Result> MarkTicketBySiteDb(int siteDbGroupId, IEnumerable<AutoMarkTicketModel> tickets)
    {
        var markTicketsCommand = new MarkTicketsCriteria
        {
            TransIds = string.Join(",", tickets.Select(x => x.TransId)),
            Status = MarkTypeConstant.ExecuteMarkTickets,
            UserId = AutoVoidConstant.ActorId,
            Ip = AutoVoidConstant.IPAddress,
            LogActionId = LogActionIdConstant.MarkTicketAddRequestTvs,
            SiteDbGroupId = siteDbGroupId,
        };

        return await repository.GetAsync<Result>(markTicketsCommand);
    }

    private async Task UpdateStatusTicket(int statusId, IEnumerable<AutoMarkTicketModel> tickets)
    {
        var command = new UpdateStatusPendingMarkTicketCommand
        {
            StatusId = statusId,
            TransIds = string.Join(",", tickets.Select(x => x.TransId))
        };

        await repository.ExecuteAsync(command);
    }
}