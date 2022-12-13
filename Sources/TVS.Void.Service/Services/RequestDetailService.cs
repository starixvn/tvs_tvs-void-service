using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services
{
    public interface RequestDetailService
    {
        Task<VoidingHistoryModel> InsertVoidingHistory(int requestId);

        Task<AfterVoidServiceModel> InsertAfterVoidServiceData(bool isAutoVoid, AfterVoidServiceModel afterVoidData);

        Task UpdateTicketVoidingHistory(int voidingHistoryId, int statusId);

        Task<(IEnumerable<RequestModel>, IEnumerable<RequestDetail>)> GetRequestById(int requestId);
    }

    public class RequestDetailServiceImp : RequestDetailService
    {
        private readonly IDynamicRepository repository;

        public RequestDetailServiceImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public async Task<VoidingHistoryModel> InsertVoidingHistory(int requestId)
        {
            var criteria = new InsertVoidingHistoryCriteria
            {
                RequestId = requestId,
                UserId = AutoVoidConstant.ActorId,
            };

            return await repository.GetAsync<VoidingHistoryModel>(criteria);
        }

        public async Task<AfterVoidServiceModel> InsertAfterVoidServiceData(bool isAutoVoid, AfterVoidServiceModel afterVoidData)
        {
            var criteria = new InsertAfterVoidServiceDataCriteria
            {
                IsAutoVoid = isAutoVoid,
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
    }
}