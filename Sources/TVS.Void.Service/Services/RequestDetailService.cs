using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Extensions;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services
{
    public interface RequestDetailService
    {
        Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>)> GetTicketsBySiteDbGroupId(
            int siteDbGroupId, AutoVoidRequestQueueModel autoVoidRequestQueue);

        Task<IEnumerable<VoidServiceModel>> InsertVoidingHistory(string broadcastSessionId, IEnumerable<VoidServiceModel> voidServiceModels);

        Task UpdateVoidService(int siteDbGroupId, int requestId, int voidingHistoryId, int statusId);

        Task UpdateVoidServiceByBroadcastSessionId(string broadcastSessionId, int statusId);

        Task<IEnumerable<VoidServiceModel>> GetVoidServiceDataForCheckStatus();

        Task<IEnumerable<AfterVoidServiceModel>> InsertAfterVoidServiceData(bool isAutoVoid, IEnumerable<AfterVoidServiceModel> afterVoidData);

        Task UpdateTicketVoidingHistory(int voidingHistoryId, int statusId);

        Task<RequestModel> GetRequestById(int requestId);
    }

    public class RequestDetailServiceImp : RequestDetailService
    {
        private readonly IDynamicRepository repository;

        public RequestDetailServiceImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public async Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>)> GetTicketsBySiteDbGroupId(int siteDbGroupId, AutoVoidRequestQueueModel autoVoidRequestQueue)
        {
            var criteria = new GetRequestDetailTicketsCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                RequestId = autoVoidRequestQueue.RequestId,
                MatchId = autoVoidRequestQueue.MatchId,
                Action = autoVoidRequestQueue.Action
            };

            var (singleTickets, parlayTickets) =
                await repository.FetchMultipleAsync<RequestDetailTicketModel, RequestDetailTicketModel>(criteria);
            return (singleTickets, parlayTickets);
        }

        public async Task<IEnumerable<VoidServiceModel>> InsertVoidingHistory(string broadcastSessionId, IEnumerable<VoidServiceModel> voidServiceModels)
        {
            var criteria = new InsertVoidingHistoryCriteria
            {
                BroadcastSessionId = broadcastSessionId,
                IsAutoVoid = true,
                UserId = AutoVoidConstant.ActorId,
                VoidServiceXml = voidServiceModels.ToVoidServiceXml()
            };

            var voidServiceData = await repository.FetchAsync<VoidServiceModel>(criteria);
            return voidServiceData;
        }

        private async Task UpdateVoidService(int? siteDbGroupId, int? requestId, int? voidingHistoryId, int statusId, string broadcastSessionId)
        {
            var criteria = new UpdateVoidServiceCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                RequestId = requestId,
                VoidingHistoryId = voidingHistoryId,
                StatusId = statusId,
                BroadcastSessionId = broadcastSessionId
            };

            await repository.ExecuteAsync(criteria);
        }

        public async Task UpdateVoidService(int siteDbGroupId, int requestId, int voidingHistoryId, int statusId)
        {
            await UpdateVoidService(siteDbGroupId, requestId, voidingHistoryId, statusId, null);
        }

        public async Task UpdateVoidServiceByBroadcastSessionId(string broadcastSessionId, int statusId)
        {
            await UpdateVoidService(null, null, null, statusId, broadcastSessionId);
        }

        public async Task<IEnumerable<VoidServiceModel>> GetVoidServiceDataForCheckStatus()
        {
            var criteria = new GetVoidServiceDataForCheckStatusCriteria();
            var voidServiceData = await repository.FetchAsync<VoidServiceModel>(criteria);
            return voidServiceData;
        }

        public async Task<IEnumerable<AfterVoidServiceModel>> InsertAfterVoidServiceData(bool isAutoVoid, IEnumerable<AfterVoidServiceModel> afterVoidData)
        {
            var criteria = new InsertAfterVoidServiceDataCriteria
            {
                IsAutoVoid = isAutoVoid,
                AfterVoidDataXml = afterVoidData.ToAfterVoidDataXml()
            };

            var result = await repository.FetchAsync<AfterVoidServiceModel>(criteria);
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

        public async Task<RequestModel> GetRequestById(int requestId)
        {
            var criteria = new GetRequestCriteria
            {
                RequestId = requestId
            };
            var request = await repository.GetAsync<RequestModel>(criteria);

            return request;
        }
    }
}