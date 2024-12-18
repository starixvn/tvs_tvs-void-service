using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.AddRequest;

public interface GetDuplicatedTicketsStage
{
    Task<DuplicatedTickets> Execute();
}

public class GetDuplicatedTicketsStageImp(IDynamicRepository repository) : GetDuplicatedTicketsStage
{
    public async Task<DuplicatedTickets> Execute()
    {
        var criteria = new GetDuplicatedTicketsCriteria();
        return await repository.GetAsync<DuplicatedTickets>(criteria);
    }
}