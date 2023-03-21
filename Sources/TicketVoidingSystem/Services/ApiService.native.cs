using System;
using System.Linq;
using System.Threading.Tasks;
using ONEbook.Product.NetPosition.Client;
using TicketVoidingSystem.Constants;
using Result = TicketVoidingSystem.Models.Result;

namespace TicketVoidingSystem.Services;

public interface ApiService
{
    Task<Result> ExecuteNetPositionApi(string transIdCrickets, string actionType);
}

public class ApiServiceImp : ApiService
{
    private readonly LoggingService loggingService;

    public ApiServiceImp(LoggingService loggingService)
    {
        this.loggingService = loggingService;
    }

    public async Task<Result> ExecuteNetPositionApi(string transIdCrickets, string actionType)
    {
        try
        {
            var ticketsId = transIdCrickets.Split(',').Select(long.Parse).ToArray();
            var service = new NPService();
            var voidAction = actionType == ActionApiType.Void ? VoidAction.Void : VoidAction.UnVoid;
            var result = service.doVoid(voidAction, ticketsId);

            var rs = result.ErrorMsg;
            if (result.ErrorDetails != null)
            {
                for (var i = 0; i < result.ErrorDetails.Count; i++)
                {
                    rs = rs == string.Empty ?
                        result.ErrorDetails[i].ErrorMsg :
                        rs + "^" + result.ErrorDetails[i].ErrorMsg;
                }
            }

            return new Result { ErrorCode = result.ErrorCode, ErrorMsg = rs };
        }
        catch (Exception exception)
        {
            await loggingService.ErrorAsync(exception.Message, exception);

            return new Result { ErrorCode = ErrorCode.Default, ErrorMsg = exception.Message };
        }
    }
}