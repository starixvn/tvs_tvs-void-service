using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services
{
    public interface TicketVoidingQueueService
    {
        Task GetQueues(int siteDbGroupId);

        Task InsertQueue(int siteDbGroupId, int voidingHistoryId, int matchId, int requestId, int issueTypeId,
            IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets);
    }

    public class TicketVoidingQueueServiceImp : TicketVoidingQueueService
    {
        private readonly IDynamicRepository repository;

        public TicketVoidingQueueServiceImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public async Task GetQueues(int siteDbGroupId)
        {
            var criteria = new GetTicketVoidingQueueCriteria
            {
                SiteDbGroupId = siteDbGroupId
            };
        }

        public async Task InsertQueue(int siteDbGroupId, int voidingHistoryId, int matchId, int requestId, int issueTypeId,
            IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets)
        {
            var criteria = new InsertTicketVoidingQueueCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                VoidingHistoryId = voidingHistoryId,
                UserId = AutoVoidConstant.ActorId,
                IP = AutoVoidConstant.IPAddress,
                MatchId = matchId,
                RequestId = requestId
            };

            await InsertQueueByBatch(criteria, singleTickets, VoidingTypeConstant.Single);
            if (issueTypeId is IssueTypeConstant.AbnormalAccount or IssueTypeConstant.AbnormalBet)
            {
                criteria.MatchId = 29;
                await InsertQueueByBatch(criteria, parlayTickets, VoidingTypeConstant.Parlay);
            }
            else
            {
                await InsertQueueByBatch(criteria, parlayTickets, VoidingTypeConstant.Parlay);
            }
        }

        private async Task InsertQueueByBatch(InsertTicketVoidingQueueCriteria criteria, IEnumerable<RequestDetailTicketModel> tickets, byte voidingType)
        {
            if (tickets.Any())
            {
                criteria.VoidingTypeId = voidingType;
                const int size = 3000;
                var number = Math.Ceiling((double)tickets.Count() / size);
                for (var i = 0; i < number; i++)
                {
                    var temp = tickets.Skip(i * size).Take(size).ToArray();
                    criteria.TransIds = string.Join(",", temp.Select(x => x.TransId));
                    criteria.TotalTicket = temp.Length;
                    await repository.ExecuteAsync(criteria);
                }
            }
        }
    }
}