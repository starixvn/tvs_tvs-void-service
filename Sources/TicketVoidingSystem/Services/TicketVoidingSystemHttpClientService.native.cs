using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using Fanex.Logging;
using TicketVoidingSystem.Models;
using TicketVoidingSystem.Utilities;

namespace TicketVoidingSystem.Services;

public interface TicketVoidingSystemHttpClientService
{
    Task SignalRUpdateRequestStatusToAllUsers(IEnumerable<RequestStatus> status);

    Task SignalRNotifyErrorMessageToUser(string userName, string message);

    Task SignalRNotifyVoidResultToUser(string userName, IEnumerable<Result> results);
}

[ExcludeFromCodeCoverage]
public class TicketVoidingSystemHttpClientServiceImp : TicketVoidingSystemHttpClientService
{
    private readonly HttpClient httpClient;
    private readonly string ticketVoidingSystemUrl;

    public TicketVoidingSystemHttpClientServiceImp(HttpClient client)
    {
        ticketVoidingSystemUrl = AppSettingService.AppSetting.TicketVoidingSystemUrl;
        httpClient = client;
        httpClient.BaseAddress = new Uri(ticketVoidingSystemUrl);
        httpClient.Timeout = TimeSpan.FromMinutes(5);
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("Accept", MediaTypeNames.Application.Json);
    }

    public async Task SignalRUpdateRequestStatusToAllUsers(IEnumerable<RequestStatus> status)
    {
        const string actionName = "SignalRApi/UpdateRequestStatusToAllUsers";
        using var response = await httpClient.PostAsync(actionName, Helper.CreateRequestStringContent(new { status }));
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        await LogService.ErrorAsync($"{ticketVoidingSystemUrl}{actionName}" +
                                    $"{Environment.NewLine}Response:{Environment.NewLine}{response}");
    }

    public async Task SignalRNotifyErrorMessageToUser(string userName, string message)
    {
        const string actionName = "SignalRApi/NotifyErrorMessageToUser";
        using var response = await httpClient.PostAsync(actionName, Helper.CreateRequestStringContent(new { userName, message }));
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        await LogService.ErrorAsync($"{ticketVoidingSystemUrl}{actionName}" +
                                    $"{Environment.NewLine}Response:{Environment.NewLine}{response}");
    }

    public async Task SignalRNotifyVoidResultToUser(string userName, IEnumerable<Result> results)
    {
        const string actionName = "SignalRApi/NotifyVoidResultToUser";
        using var response = await httpClient.PostAsync(actionName, Helper.CreateRequestStringContent(new { userName, results }));
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        await LogService.ErrorAsync($"{ticketVoidingSystemUrl}{actionName}" +
                                    $"{Environment.NewLine}Response:{Environment.NewLine}{response}");
    }
}