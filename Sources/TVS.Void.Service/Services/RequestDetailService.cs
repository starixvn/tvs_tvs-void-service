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
        Task<VoidingHistoryModel> InsertVoidingHistory(int requestId);

        Task<IEnumerable<AfterVoidServiceModel>> InsertAfterVoidServiceData(bool isAutoVoid, IEnumerable<AfterVoidServiceModel> afterVoidData);

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
                IsAutoVoid = true,
                UserId = AutoVoidConstant.ActorId,
            };

            return await repository.GetAsync<VoidingHistoryModel>(criteria);
        }

        public async Task<IEnumerable<AfterVoidServiceModel>> InsertAfterVoidServiceData(bool isAutoVoid, IEnumerable<AfterVoidServiceModel> afterVoidData)
        {
            var criteria = new InsertAfterVoidServiceDataCriteria
            {
                IsAutoVoid = isAutoVoid,
                AfterVoidDataXml = afterVoidData.ToAfterVoidDataXml(),
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