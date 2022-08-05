using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Extensions;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services
{
    public interface RequestDetailService
    {
        Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>,
            IEnumerable<RequestDetailModel>)> CheckTicketsAllowToVoid(int siteDbGroupId, int matchId);

        Task<IEnumerable<VoidServiceModel>> InsertVoidServiceData(string broadcastSessionId, int siteDbGroupId,
            IEnumerable<RequestDetailModel> requests);
    }

    public class RequestDetailServiceImp : RequestDetailService
    {
        private readonly IDynamicRepository repository;

        public RequestDetailServiceImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        private async Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>)> GetTicketsBySiteDbGroupId(int siteDbGroupId, int matchId)
        {
            var criteria = new GetRequestDetailTicketsCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                MatchId = matchId
            };

            var (singleTickets, parlayTickets) =
                await repository.FetchMultipleAsync<RequestDetailTicketModel, RequestDetailTicketModel>(criteria);
            return (singleTickets, parlayTickets);
        }

        public async Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailModel>)> CheckTicketsAllowToVoid(int siteDbGroupId, int matchId)
        {
            var (singleTickets, parlayTickets) = await GetTicketsBySiteDbGroupId(siteDbGroupId, matchId);
            var criteria = new CheckTicketsAllowToVoidCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                SingleTickets = singleTickets.ToTicketsXml(),
                ParlayTickets = parlayTickets.ToTicketsXml()
            };

            var (validSingleTickets, validParlayTickets, validRequests) =
                await repository.FetchMultipleAsync<RequestDetailTicketModel, RequestDetailTicketModel, RequestDetailModel>(criteria);
            return (validSingleTickets, validParlayTickets, validRequests);
        }

        public async Task<IEnumerable<VoidServiceModel>> InsertVoidServiceData(string broadcastSessionId, int siteDbGroupId, IEnumerable<RequestDetailModel> requests)
        {
            var criteria = new InsertVoidServiceDataCriteria
            {
                BroadcastSessionId = broadcastSessionId,
                SiteDbGroupId = siteDbGroupId,
                IsAutoVoid = true,
                RequestIdXml = requests.ToRequestIdXml()
            };

            var voidServiceData = await repository.FetchAsync<VoidServiceModel>(criteria);
            return voidServiceData;
        }

        public async Task InsertTicketVoidingQueue(int voidingHistoryId, int matchId, int requestId,
            IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets)
        {
            var criteria = new InsertTicketVoidingQueueCriteria
            {
                VoidingHistoryId = voidingHistoryId,
                UserId = AutoVoidConstant.ActorId,
                IP = AutoVoidConstant.IPAddress,
                MatchId = matchId,
                RequestId = requestId
            };

            //await InsertQueueCommand(criteria, singleTickets, VoidingTypeConstant.Single);
            //if (query.IssueTypeId == IssueTypeConstant.AbnormalAccount)
            //{
            //    var commandParlayAbnormalAccount = command;
            //    commandParlayAbnormalAccount.MatchId = 29;
            //    await InsertQueueCommand(commandParlayAbnormalAccount, query.ParlayTransIds, VoidingTypeConstant.Parlay);
            //}
            //else
            //{
            //    await InsertQueueCommand(command, query.ParlayTransIds, VoidingTypeConstant.Parlay);
            //}
        }

        private async Task InsertQueueCommand(InsertTicketVoidingQueueCriteria command, string transIds, byte type)
        {
            if (!string.IsNullOrWhiteSpace(transIds))
            {
                command.VoidingTypeId = type;
                await InsertQueueByBatch(command, transIds);
            }
        }

        private async Task InsertQueueByBatch(InsertTicketVoidingQueueCriteria command, string transIds)
        {
            var transList = transIds.Split(',').ToList();
            const int size = 3000;
            var number = Math.Ceiling((double)transList.Count / size);
            for (var i = 0; i < number; i++)
            {
                var temp = transList.Skip(i * size).Take(size).ToArray();
                command.TransIds = string.Join(",", temp);
                command.TotalTicket = temp.Length;
                await repository.ExecuteAsync(command);
            }
        }
    }
}