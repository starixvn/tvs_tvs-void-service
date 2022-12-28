using NSubstitute;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.Void;

namespace TVS.Void.Service.Tests.Services.Void
{
    public class ProcessVoidQueuesStageImpTests
    {
        [Theory]
        [InlineData(ExecuteStatus.Failed)]
        [InlineData(ExecuteStatus.Done)]
        public async Task Execute_WhenQueuesAllDoneOrFailed_ShouldNotProcessVoidQueue(int executeStatus)
        {
            // Arrange
            var fakeQueues = new List<TicketVoidingQueueModel> { new() { StatusId = executeStatus } };
            var ticketVoidingQueueService = Substitute.For<TicketVoidingQueueService>();
            var service = new ProcessVoidQueuesStageImp(ticketVoidingQueueService);

            // Act
            var actual = await service.Execute(1, fakeQueues);

            // Assert
            Assert.Equal(fakeQueues.Count, actual.Count());
            Assert.Equal(fakeQueues.First().StatusId, actual.First().StatusId);
        }

        [Theory]
        [InlineData(ExecuteStatus.Running)]
        public async Task Execute_WhenQueuesAllRunning_ShouldProcessVoidQueue(int executeStatus)
        {
            // Arrange
            var fakeQueues = new List<TicketVoidingQueueModel> { new() { StatusId = executeStatus, QueueId = 223 } };
            var fakeProcessQueuesResult = new TicketVoidingQueueModel { VoidingHistoryId = 113, StatusId = 2 };
            var ticketVoidingQueueService = Substitute.For<TicketVoidingQueueService>();
            ticketVoidingQueueService.ProcessVoidQueue(default, default).ReturnsForAnyArgs(fakeProcessQueuesResult);
            var service = new ProcessVoidQueuesStageImp(ticketVoidingQueueService);

            // Act
            var actual = await service.Execute(1, fakeQueues);

            // Assert
            await ticketVoidingQueueService.Received(1).ProcessVoidQueue(Arg.Is<int>(x => x == 1),
                Arg.Is<int>(x => x == fakeQueues.First().QueueId));
            Assert.Equal(fakeQueues.Count, actual.Count());
            Assert.Equal(fakeProcessQueuesResult, actual.First());
        }
    }
}