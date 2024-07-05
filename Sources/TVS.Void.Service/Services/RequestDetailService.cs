using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services;

public interface RequestDetailService
{
    Task<IEnumerable<VoidingHistoryModel>> InsertVoidingHistory(string requestsJson, int siteDbGroupId, string broadcastSessionId, string voidDataXml);

    Task<AfterVoidServiceModel> InsertAfterVoidServiceData(bool isRunAutoAfterVoid, AfterVoidServiceModel afterVoidData);

    Task UpdateTicketVoidingHistory(int voidingHistoryId, int statusId);

    Task<(IEnumerable<RequestModel>, IEnumerable<RequestDetail>)> GetRequestById(int requestId);

    Task<VoidDataXmlModel> GetVoidDataOfVoidingHistory(int voidingHistoryId);
}

public class RequestDetailServiceImp : RequestDetailService
{
    private readonly IDynamicRepository repository;

    public RequestDetailServiceImp(IDynamicRepository repository)
    {
        this.repository = repository;
    }

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

    public async Task<AfterVoidServiceModel> InsertAfterVoidServiceData(bool isRunAutoAfterVoid, AfterVoidServiceModel afterVoidData)
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
        return result;
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
}