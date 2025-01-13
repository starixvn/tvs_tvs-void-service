using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.AddRequest;

public interface CheckDuplicatedTicketsStage
{
    Task<IEnumerable<CheckDuplicatedTicketsResult>> Execute(string transInfoJson);
}

public class CheckDuplicatedTicketsStageImp(IDynamicRepository repository) : CheckDuplicatedTicketsStage
{
    public async Task<IEnumerable<CheckDuplicatedTicketsResult>> Execute(string transInfoJson)
    {
        var criteria = new CheckDuplicateTicketsCriteria
        {
            TransInfoJson = transInfoJson
        };

        var (_, _, results) = await repository.FetchMultipleAsync<object, object, CheckDuplicatedTicketsResult>(criteria);

        return results;
    }
}