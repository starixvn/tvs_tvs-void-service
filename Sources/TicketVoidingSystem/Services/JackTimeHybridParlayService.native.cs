using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Fanex.Logging;
using Microsoft.Extensions.Configuration;
using TicketVoidingSystem.Models;

namespace TicketVoidingSystem.Services;

public interface JackTimeHybridParlayService
{
    Task<bool> Execute(HybridParlayApiModel requestModel, bool isVoid);
}

public class JackTimeHybridParlayServiceImp : JackTimeHybridParlayService
{
    private readonly HttpClient httpClient;
    private readonly string jackTimeHybridParlayUrl;

    public JackTimeHybridParlayServiceImp(HttpClient client, IConfiguration configuration)
    {
        jackTimeHybridParlayUrl = AppSettingService.AppSetting.JackTimeHybridParlayUrl ?? string.Empty;
        httpClient = client;
        httpClient.BaseAddress = new Uri(jackTimeHybridParlayUrl);
        httpClient.Timeout = TimeSpan.FromMinutes(5);
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("Accept", MediaTypeNames.Application.Json);
    }

    private static StringContent CreateRequestStringContent<T>(T data)
    {
        var dataJson = new StringContent(
            JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);

        return dataJson;
    }

    public async Task<bool> Execute(HybridParlayApiModel requestModel, bool isVoid)
    {
        var actionName = isVoid ? "VoidLegs" : "UndoVoidLegs";
        using var response = await httpClient.PostAsync(actionName, CreateRequestStringContent(requestModel));
        if (response.IsSuccessStatusCode)
        {
            await LogService.InfoAsync(await CreateLogResponse(actionName, response));
            return true;
        }

        await LogService.ErrorAsync(await CreateLogResponse(actionName, response));
        return false;
    }

    private async Task<string> CreateLogResponse(string actionName, HttpResponseMessage response)
    {
        var requestContent = response.RequestMessage?.Content != null ? await response.RequestMessage.Content.ReadAsStringAsync() : string.Empty;
        var responseContent = await response.Content.ReadAsStringAsync();

        return $"{jackTimeHybridParlayUrl}{actionName}" +
               $"{Environment.NewLine}RequestContent:{Environment.NewLine}{requestContent}" +
               $"{Environment.NewLine}Response:{Environment.NewLine}{response}" +
               $"{Environment.NewLine}ResponseContent:{Environment.NewLine}{responseContent}";
    }
}