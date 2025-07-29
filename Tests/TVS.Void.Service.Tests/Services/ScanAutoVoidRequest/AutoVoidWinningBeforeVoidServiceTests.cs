using NSubstitute;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Tests.Services.ScanAutoVoidRequest;

public class AutoVoidWinningBeforeVoidServiceTests
{
    private readonly GetTicketsStage getTicketsStage;
    private readonly CheckVoidWinningRequestQueueValidStage checkRequestQueueValidStage;
    private readonly AutoVoidWinningBeforeVoidServiceImp service;

    public AutoVoidWinningBeforeVoidServiceTests()
    {
        getTicketsStage = Substitute.For<GetTicketsStage>();
        checkRequestQueueValidStage = Substitute.For<CheckVoidWinningRequestQueueValidStage>();
        service = new AutoVoidWinningBeforeVoidServiceImp(getTicketsStage, checkRequestQueueValidStage);
    }

    [Fact]
    public async Task CheckVoidWinningRequestQueueValid_ValidQueue_ReturnExpected()
    {
        // Arrange
        const int siteDbGroupId = 1;
        var queue = new AutoVoidRequestQueueModel
        {
            QueueId = 1,
            RequestIds = "1,2",
            Action = "CAL"
        };
        const bool expectedIsValid = true;
        const string expectedRequestIdsChangedAction = "1";

        checkRequestQueueValidStage.Execute(siteDbGroupId, queue)
            .Returns((expectedIsValid, expectedRequestIdsChangedAction));

        // Act
        var (isValid, requestIdsChangedAction) = await service.CheckVoidWinningRequestQueueValid(siteDbGroupId, queue);

        // Assert
        await checkRequestQueueValidStage.Received(1).Execute(siteDbGroupId, queue);
        Assert.Equal(expectedIsValid, isValid);
        Assert.Equal(expectedRequestIdsChangedAction, requestIdsChangedAction);
    }

    [Fact]
    public async Task CheckVoidWinningRequestQueueValid_InvalidQueue_ReturnExpected()
    {
        // Arrange
        const int siteDbGroupId = 1;
        var queue = new AutoVoidRequestQueueModel
        {
            QueueId = 1,
            RequestIds = "1,2",
            Action = "CAL"
        };
        const bool expectedIsValid = false;
        const string expectedRequestIdsChangedAction = "";

        checkRequestQueueValidStage.Execute(siteDbGroupId, queue)
            .Returns((expectedIsValid, expectedRequestIdsChangedAction));

        // Act
        var (isValid, requestIdsChangedAction) = await service.CheckVoidWinningRequestQueueValid(siteDbGroupId, queue);

        // Assert
        await checkRequestQueueValidStage.Received(1).Execute(siteDbGroupId, queue);
        Assert.Equal(expectedIsValid, isValid);
        Assert.Equal(expectedRequestIdsChangedAction, requestIdsChangedAction);
    }

    [Fact]
    public async Task CheckVoidWinningRequestQueueValid_EmptyRequestIds_ReturnExpected()
    {
        // Arrange
        const int siteDbGroupId = 1;
        var queue = new AutoVoidRequestQueueModel
        {
            QueueId = 1,
            RequestIds = "",
            Action = "CAL"
        };
        const bool expectedIsValid = true;
        const string expectedRequestIdsChangedAction = "";

        checkRequestQueueValidStage.Execute(siteDbGroupId, queue)
            .Returns((expectedIsValid, expectedRequestIdsChangedAction));

        // Act
        var (isValid, requestIdsChangedAction) = await service.CheckVoidWinningRequestQueueValid(siteDbGroupId, queue);

        // Assert
        await checkRequestQueueValidStage.Received(1).Execute(siteDbGroupId, queue);
        Assert.Equal(expectedIsValid, isValid);
        Assert.Equal(expectedRequestIdsChangedAction, requestIdsChangedAction);
    }

    [Fact]
    public async Task CheckVoidWinningRequestQueueValid_NullQueue_ReturnExpected()
    {
        // Arrange
        const int siteDbGroupId = 1;
        AutoVoidRequestQueueModel? queue = null;
        const bool expectedIsValid = false;
        const string expectedRequestIdsChangedAction = "";

        checkRequestQueueValidStage.Execute(siteDbGroupId, queue!)
            .Returns((expectedIsValid, expectedRequestIdsChangedAction));

        // Act
        var (isValid, requestIdsChangedAction) = await service.CheckVoidWinningRequestQueueValid(siteDbGroupId, queue!);

        // Assert
        await checkRequestQueueValidStage.Received(1).Execute(siteDbGroupId, queue!);
        Assert.Equal(expectedIsValid, isValid);
        Assert.Equal(expectedRequestIdsChangedAction, requestIdsChangedAction);
    }

    [Fact]
    public async Task GetTickets_ValidData_ReturnExpected()
    {
        // Arrange
        const int siteDbGroupId = 1;
        var queue = new AutoVoidRequestQueueModel
        {
            QueueId = 1,
            RequestIds = "1,2",
            Action = "CAL"
        };
        var logData = new List<string> { "log1", "log2" };
        const bool expectedIsContinueFlow = true;
        var expectedOutputData = new GroupTicketsInRequest
        {
            SingleTickets = [],
            ParlayTickets = [],
            CricketTickets = [],
            CancelCOTickets = []
        };

        getTicketsStage.Execute(siteDbGroupId, queue, logData)
            .Returns((expectedIsContinueFlow, expectedOutputData));

        // Act
        var (isContinueFlow, outputData) = await service.GetTickets(siteDbGroupId, queue, logData);

        // Assert
        await getTicketsStage.Received(1).Execute(siteDbGroupId, queue, logData);
        Assert.Equal(expectedIsContinueFlow, isContinueFlow);
        Assert.Equal(expectedOutputData, outputData);
    }

    [Fact]
    public async Task GetTickets_NoContinueFlow_ReturnExpected()
    {
        // Arrange
        const int siteDbGroupId = 1;
        var queue = new AutoVoidRequestQueueModel
        {
            QueueId = 1,
            RequestIds = "1,2",
            Action = "CAL"
        };
        var logData = new List<string> { "log1", "log2" };
        const bool expectedIsContinueFlow = false;
        GroupTicketsInRequest? expectedOutputData = null;

        getTicketsStage.Execute(siteDbGroupId, queue, logData)
            .Returns((expectedIsContinueFlow, expectedOutputData));

        // Act
        var (isContinueFlow, outputData) = await service.GetTickets(siteDbGroupId, queue, logData);

        // Assert
        await getTicketsStage.Received(1).Execute(siteDbGroupId, queue, logData);
        Assert.Equal(expectedIsContinueFlow, isContinueFlow);
        Assert.Equal(expectedOutputData, outputData);
    }

    [Fact]
    public async Task GetTickets_EmptyLogData_ReturnExpected()
    {
        // Arrange
        const int siteDbGroupId = 1;
        var queue = new AutoVoidRequestQueueModel
        {
            QueueId = 1,
            RequestIds = "1,2",
            Action = "CAL"
        };
        var logData = new List<string>();
        const bool expectedIsContinueFlow = true;
        var expectedOutputData = new GroupTicketsInRequest
        {
            SingleTickets = [],
            ParlayTickets = [],
            CricketTickets = [],
            CancelCOTickets = []
        };

        getTicketsStage.Execute(siteDbGroupId, queue, logData)
            .Returns((expectedIsContinueFlow, expectedOutputData));

        // Act
        var (isContinueFlow, outputData) = await service.GetTickets(siteDbGroupId, queue, logData);

        // Assert
        await getTicketsStage.Received(1).Execute(siteDbGroupId, queue, logData);
        Assert.Equal(expectedIsContinueFlow, isContinueFlow);
        Assert.Equal(expectedOutputData, outputData);
    }

    [Fact]
    public async Task GetTickets_NullLogData_ReturnExpected()
    {
        // Arrange
        const int siteDbGroupId = 1;
        var queue = new AutoVoidRequestQueueModel
        {
            QueueId = 1,
            RequestIds = "1,2",
            Action = "CAL"
        };
        IList<string>? logData = null;
        const bool expectedIsContinueFlow = true;
        var expectedOutputData = new GroupTicketsInRequest
        {
            SingleTickets = [],
            ParlayTickets = [],
            CricketTickets = [],
            CancelCOTickets = []
        };

        getTicketsStage.Execute(siteDbGroupId, queue, logData!)
            .Returns((expectedIsContinueFlow, expectedOutputData));

        // Act
        var (isContinueFlow, outputData) = await service.GetTickets(siteDbGroupId, queue, logData!);

        // Assert
        await getTicketsStage.Received(1).Execute(siteDbGroupId, queue, logData!);
        Assert.Equal(expectedIsContinueFlow, isContinueFlow);
        Assert.Equal(expectedOutputData, outputData);
    }

    [Fact]
    public async Task GetTickets_EmptyRequestIds_ReturnExpected()
    {
        // Arrange
        const int siteDbGroupId = 1;
        var queue = new AutoVoidRequestQueueModel
        {
            QueueId = 1,
            RequestIds = "",
            Action = "CAL"
        };
        var logData = new List<string> { "log1" };
        const bool expectedIsContinueFlow = false;
        GroupTicketsInRequest? expectedOutputData = null;

        getTicketsStage.Execute(siteDbGroupId, queue, logData)
            .Returns((expectedIsContinueFlow, expectedOutputData));

        // Act
        var (isContinueFlow, outputData) = await service.GetTickets(siteDbGroupId, queue, logData);

        // Assert
        await getTicketsStage.Received(1).Execute(siteDbGroupId, queue, logData);
        Assert.Equal(expectedIsContinueFlow, isContinueFlow);
        Assert.Equal(expectedOutputData, outputData);
    }

    [Fact]
    public async Task GetTickets_NullQueue_ReturnExpected()
    {
        // Arrange
        const int siteDbGroupId = 1;
        AutoVoidRequestQueueModel? queue = null;
        var logData = new List<string> { "log1" };
        const bool expectedIsContinueFlow = false;
        GroupTicketsInRequest? expectedOutputData = null;

        getTicketsStage.Execute(siteDbGroupId, queue!, logData)
            .Returns((expectedIsContinueFlow, expectedOutputData));

        // Act
        var (isContinueFlow, outputData) = await service.GetTickets(siteDbGroupId, queue!, logData);

        // Assert
        await getTicketsStage.Received(1).Execute(siteDbGroupId, queue!, logData);
        Assert.Equal(expectedIsContinueFlow, isContinueFlow);
        Assert.Equal(expectedOutputData, outputData);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task CheckVoidWinningRequestQueueValid_DifferentSiteDbGroupIds_ReturnExpected(int siteDbGroupId)
    {
        // Arrange
        var queue = new AutoVoidRequestQueueModel
        {
            QueueId = 1,
            RequestIds = "1,2",
            Action = "CAL"
        };
        const bool expectedIsValid = true;
        const string expectedRequestIdsChangedAction = "";

        checkRequestQueueValidStage.Execute(siteDbGroupId, queue)
            .Returns((expectedIsValid, expectedRequestIdsChangedAction));

        // Act
        var (isValid, requestIdsChangedAction) = await service.CheckVoidWinningRequestQueueValid(siteDbGroupId, queue);

        // Assert
        await checkRequestQueueValidStage.Received(1).Execute(siteDbGroupId, queue);
        Assert.Equal(expectedIsValid, isValid);
        Assert.Equal(expectedRequestIdsChangedAction, requestIdsChangedAction);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetTickets_DifferentSiteDbGroupIds_ReturnExpected(int siteDbGroupId)
    {
        // Arrange
        var queue = new AutoVoidRequestQueueModel
        {
            QueueId = 1,
            RequestIds = "1,2",
            Action = "CAL"
        };
        var logData = new List<string> { "log1" };
        const bool expectedIsContinueFlow = true;
        var expectedOutputData = new GroupTicketsInRequest
        {
            SingleTickets = [],
            ParlayTickets = [],
            CricketTickets = [],
            CancelCOTickets = []
        };

        getTicketsStage.Execute(siteDbGroupId, queue, logData)
            .Returns((expectedIsContinueFlow, expectedOutputData));

        // Act
        var (isContinueFlow, outputData) = await service.GetTickets(siteDbGroupId, queue, logData);

        // Assert
        await getTicketsStage.Received(1).Execute(siteDbGroupId, queue, logData);
        Assert.Equal(expectedIsContinueFlow, isContinueFlow);
        Assert.Equal(expectedOutputData, outputData);
    }
}