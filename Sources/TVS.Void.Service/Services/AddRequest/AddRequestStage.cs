using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.AddRequest;

public interface AddRequestStage
{
    Task<int> Execute(CheckDuplicatedTicketsResult param, IList<string> logData);
}

public class AddRequestStageImp : AddRequestStage
{
    private readonly IDynamicRepository repository;

    public AddRequestStageImp(IDynamicRepository repository)
    {
        this.repository = repository;
    }

    public async Task<int> Execute(CheckDuplicatedTicketsResult param, IList<string> logData)
    {
        var command = new AddRequestCommand
        {
            IssueTypeId = IssueTypeConstant.DuplicateTickets,
            MatchId = param.MatchId,
            SportType = param.SportType,
            ActionId = VoidActionTypeConstant.AutoVoidInstantId,
            DecidedBy = AutoVoidConstant.ActorName,
            UserId = AutoVoidConstant.ActorId,
            IP = AutoVoidConstant.IPAddress,
            IsTest = false,
            TransInfoJson = param.TransInfoJson,
            DuplicateTicketsInfo = BuildDuplicateTicketsXml(param)
        };

        await repository.ExecuteAsync(command);
        logData.Add($"Added RequestId: {command.RequestId}");
        return command.RequestId;
    }

    private static string BuildDuplicateTicketsXml(CheckDuplicatedTicketsResult param)
    {
        var stringBuilder = $"<r FilterBy='{GetRequestDuplicatedFilterBy(param.MatchId)}' "
                            + $"MatchId='{param.MatchId}' "
                            + $"SubIssueTypeId='{SubIssueTypeConstant.DuplicateTickets}' />";

        return $"<root>{stringBuilder.Trim()}</root>";
    }

    private static int GetRequestDuplicatedFilterBy(int matchId)
    {
        const int parlayMatchId = 29;
        return matchId == parlayMatchId ? 1 : 0;
    }
}