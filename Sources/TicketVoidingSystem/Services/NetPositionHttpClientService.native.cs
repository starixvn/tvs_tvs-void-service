using System;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Models;
using TicketVoidingSystem.Utilities;
using Result = TicketVoidingSystem.Models.Result;

namespace TicketVoidingSystem.Services;

public interface NetPositionHttpClientService
{
    Task<Result> PostRequest(string transIdCrickets, string actionType);
}

public class NetPositionHttpClientServiceImp : NetPositionHttpClientService
{
    private readonly HttpClient httpClient;
    private readonly string netPositionUrl;
    private readonly LoggingService loggingService;

    public NetPositionHttpClientServiceImp(HttpClient client, IConfiguration configuration, LoggingService loggingService)
    {
        netPositionUrl = configuration.GetSection("NetPositionUrl").Value;
        httpClient = client;
        httpClient.BaseAddress = new Uri(netPositionUrl);
        httpClient.Timeout = TimeSpan.FromMinutes(5);
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("Accept", MediaTypeNames.Application.Json);
        this.loggingService = loggingService;
    }

    public async Task<Result> PostRequest(string transIdCrickets, string actionType)
    {
        try
        {
            var requestModel = new NetPositionRequestModel
            {
                Action = actionType,
                TransId = transIdCrickets.Split(',').Select(long.Parse).ToArray()
            };
            const string actionName = "void";
            var response = await httpClient.PostAsync(actionName, Helper.CreateRequestStringContent(requestModel));
            var result = await Helper.DeserializeResponseStream<Result>(response);

            if (!response.IsSuccessStatusCode)
            {
                await loggingService.ErrorAsync($"{netPositionUrl}{actionName}" +
                                                $"{Environment.NewLine}Response:{Environment.NewLine}{response}");
            }

            return result;
        }
        catch (Exception exception)
        {
            await loggingService.ErrorAsync(exception.Message, exception);

            return new Result { ErrorCode = ErrorCode.Default, ErrorMsg = exception.Message };
        }
    }
}