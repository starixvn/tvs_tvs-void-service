using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TicketVoidingSystem.Services;
using TicketVoidingSystem.Utilities;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services;

public interface RequestDetailService
{
    Task<IEnumerable<VoidingHistoryModel>> InsertVoidingHistory(string requestsJson, int siteDbGroupId, string broadcastSessionId, string voidDataXml);

    Task InsertAfterVoidServiceData(bool isRunAutoAfterVoid, AfterVoidServiceModel afterVoidData, IList<string> logData);

    Task UpdateTicketVoidingHistory(int voidingHistoryId, int statusId);

    Task<(IEnumerable<RequestModel>, IEnumerable<RequestDetail>)> GetRequestById(int requestId);

    Task<VoidDataXmlModel> GetVoidDataOfVoidingHistory(int voidingHistoryId);

    Task InsertLetsTalk(int requestId, int issueTypeId, string broadcastSessionId, int statusId, IList<string> logData);
}

public class RequestDetailServiceImp(IDynamicRepository repository, LoggingService loggingService) : RequestDetailService
{
    public async Task<IEnumerable<VoidingHistoryModel>> InsertVoidingHistory(string requestsJson, int siteDbGroupId, string broadcastSessionId, string voidDataXml)
    {
        var criteria = new InsertVoidingHistoryCriteria
        {
            RequestsJson = requestsJson,
            UserId = AutoVoidConstant.ActorId,
            SiteDbGroupId = siteDbGroupId,
            BroadcastSessionId = broadcastSessionId,
            VoidData = voidDataXml
        };

        return await repository.FetchAsync<VoidingHistoryModel>(criteria);
    }

    public async Task InsertAfterVoidServiceData(bool isRunAutoAfterVoid, AfterVoidServiceModel afterVoidData, IList<string> logData)
    {
        var criteria = new InsertAfterVoidServiceDataCriteria
        {
            IsAutoVoid = isRunAutoAfterVoid,
            RequestId = afterVoidData.RequestId,
            VoidingHistoryId = afterVoidData.VoidingHistoryId,
            MatchId = afterVoidData.MatchId,
            NegativeBalanceUsers = afterVoidData.NegativeBalanceUsers
        };

        var result = await repository.GetAsync<AfterVoidServiceModel>(criteria);
        logData.Add($"{nameof(RequestDetailService.InsertAfterVoidServiceData)}: {JsonSerializer.Serialize(result)}");
    }

    public async Task UpdateTicketVoidingHistory(int voidingHistoryId, int statusId)
    {
        var command = new UpdateTicketVoidingHistoryCriteria
        {
            VoidingHistoryId = voidingHistoryId,
            StatusId = statusId,
            ReasonId = null,
            MessageId = null
        };

        await repository.ExecuteAsync(command);
    }

    public async Task<(IEnumerable<RequestModel>, IEnumerable<RequestDetail>)> GetRequestById(int requestId)
    {
        var criteria = new GetRequestCriteria
        {
            RequestId = requestId
        };

        return await repository.FetchMultipleAsync<RequestModel, RequestDetail>(criteria);
    }

    public async Task<VoidDataXmlModel> GetVoidDataOfVoidingHistory(int voidingHistoryId)
    {
        var allVoidDataInCurrentVoidTimes = await GetAllVoidDataInCurrentVoidTimes(voidingHistoryId);
        return allVoidDataInCurrentVoidTimes.First(x => voidingHistoryId == x.VoidingHistoryId);
    }

    private async Task<IEnumerable<VoidDataXmlModel>> GetAllVoidDataInCurrentVoidTimes(int voidingHistoryId)
    {
        var criteria = new GetVoidDataOfVoidingHistoryCriteria
        {
            VoidingHistoryIds = voidingHistoryId.ToString()
        };

        return await repository.FetchAsync<VoidDataXmlModel>(criteria);
    }

    public async Task InsertLetsTalk(int requestId, int issueTypeId, string broadcastSessionId, int statusId, IList<string> logData)
    {
        try
        {
            if (!Helper.IsCanSendLetsTalk(issueTypeId))
            {
                return;
            }

            var command = new InsertLetstalkCommand
            {
                RequestId = requestId,
                BroadcastSessionId = broadcastSessionId,
                StatusId = statusId
            };

            await repository.ExecuteAsync(command);
            logData.Add($"{nameof(RequestDetailService.InsertLetsTalk)}: Done");
        }
        catch (Exception e)
        {
            await loggingService.ErrorAsync(e);
            logData.Add($"{nameof(RequestDetailService.InsertLetsTalk)}: Failed");
        }
    }
}