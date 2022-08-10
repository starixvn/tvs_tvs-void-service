using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Extensions;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanningAutoVoidRequest
{
    public interface ReCalculateTicketsStage
    {
        Task Execute(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTicketsResettle, IEnumerable<RequestDetailTicketModel> parlayTicketsResettle);
    }

    public class ReCalculateTicketsStageImp : ReCalculateTicketsStage
    {
        private readonly IDynamicRepository repository;

        public ReCalculateTicketsStageImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public async Task Execute(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTicketsResettle, IEnumerable<RequestDetailTicketModel> parlayTicketsResettle)
        {
            // TODO: Check if this step fail -> re execute
            if (singleTicketsResettle != null || parlayTicketsResettle != null)
            {
                var criteria = new ReCalculateTicketsCriteria
                {
                    SiteDbGroupId = siteDbGroupId,
                    SingleTicketsXml = singleTicketsResettle.ToTicketsXml(),
                    ParlayTicketsXml = parlayTicketsResettle.ToTicketsXml()
                };

                await repository.ExecuteAsync(criteria);
            }
        }
    }
}