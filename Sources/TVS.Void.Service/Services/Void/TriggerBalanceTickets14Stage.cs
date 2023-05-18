using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.Void;

public interface TriggerBalanceTickets14Stage
{
    Task Execute(TicketVoidingQueueModel queue, IList<string> logData);
}

public class TriggerBalanceTickets14StageImp : TriggerBalanceTickets14Stage
{
    private readonly IDynamicRepository repository;

    public TriggerBalanceTickets14StageImp(IDynamicRepository repository)
    {
        this.repository = repository;
    }

    public async Task Execute(TicketVoidingQueueModel queue, IList<string> logData)
    {
        if (!string.IsNullOrWhiteSpace(queue.BeforeTransInfoJson) && !string.IsNullOrWhiteSpace(queue.AfterTransInfoJson))
        {
            await ExecuteTriggerBalanceMainDb(queue.BeforeTransInfoJson, queue.AfterTransInfoJson);

            if (AppSettingService.AppSetting.IsPRODEnvironment)
            {
                await ExecuteTriggerBalanceBackupDb(queue.BeforeTransInfoJson, queue.AfterTransInfoJson);
            }

            logData.Add($"{nameof(TriggerBalanceTickets14Stage)} - {nameof(queue.QueueId)}: {queue.QueueId}"
                        + $" - {nameof(queue.BeforeTransInfoJson)}: {queue.BeforeTransInfoJson} - {nameof(queue.AfterTransInfoJson)}: {queue.AfterTransInfoJson}");
        }
    }

    private async Task ExecuteTriggerBalanceMainDb(string beforeTransInfoJson, string afterTransInfoJson)
    {
        var criteria = new TriggerBalanceTickets14Criteria
        {
            BeforeTransInfoJson = beforeTransInfoJson,
            AfterTransInfoJson = afterTransInfoJson
        };

        await repository.ExecuteAsync(criteria);
    }

    private async Task ExecuteTriggerBalanceBackupDb(string beforeTransInfoJson, string afterTransInfoJson)
    {
        var criteria = new TriggerBalanceTickets14BackupCriteria
        {
            BeforeTransInfoJson = beforeTransInfoJson,
            AfterTransInfoJson = afterTransInfoJson
        };

        await repository.ExecuteAsync(criteria);
    }
}