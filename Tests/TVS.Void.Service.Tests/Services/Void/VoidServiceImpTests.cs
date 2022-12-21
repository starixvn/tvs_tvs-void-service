using NSubstitute;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.Void;

namespace TVS.Void.Service.Tests.Services.Void
{
    public class VoidServiceImpTests
    {
        [Fact]
        public async Task Execute_WhenNotHaveAnyQueues_ShouldReturn()
        {
            // Arrange
            const int fakeSiteDbGroupId = 2;
            var ticketVoidingQueueService = Substitute.For<TicketVoidingQueueService>();
            var processVoidQueuesStage = Substitute.For<ProcessVoidQueuesStage>();
            ticketVoidingQueueService.GetQueues(default, default).ReturnsForAnyArgs(new List<TicketVoidingQueueModel>());
            var service = new VoidServiceImp(ticketVoidingQueueService, default, processVoidQueuesStage, default,
                default, default);
            // Act
            await service.Execute(fakeSiteDbGroupId);

            // Assert
            await ticketVoidingQueueService.Received(1).GetQueues(Arg.Is<int>(x => x == fakeSiteDbGroupId), Arg.Is<int?>(x => x == null));
            await processVoidQueuesStage.DidNotReceiveWithAnyArgs().Execute(default, default);
        }

        [Fact]
        public async Task Execute_WhenHaveQueues_ShouldExecuteNormalFlow()
        {
            // Arrange
            const int fakeSiteDbGroupId = 2;
            var ticketVoidingQueueService = Substitute.For<TicketVoidingQueueService>();
            var processVoidQueuesStage = Substitute.For<ProcessVoidQueuesStage>();
            ticketVoidingQueueService.GetQueues(default, default).ReturnsForAnyArgs(new List<TicketVoidingQueueModel>());
            var service = new VoidServiceImp(ticketVoidingQueueService, default, processVoidQueuesStage, default,
                default, default);
            // Act
            await service.Execute(fakeSiteDbGroupId);

            // Assert
            await ticketVoidingQueueService.Received(1).GetQueues(Arg.Is<int>(x => x == fakeSiteDbGroupId), Arg.Is<int?>(x => x == null));
            await processVoidQueuesStage.DidNotReceiveWithAnyArgs().Execute(default, default);
        }
    }
}