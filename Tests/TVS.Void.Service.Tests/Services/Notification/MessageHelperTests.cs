using TicketVoidingSystem.Constants;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services.Notification;

namespace TVS.Void.Service.Tests.Services.Notification;

public class MessageHelperTests
{
    [Fact]
    public void CreateVoidServiceFailed_WithVoidAction_ReturnsCorrectMessage()
    {
        // Arrange
        const int requestId = 123;
        var voidQueueResults = new List<TicketVoidingQueueModel>();
        const bool isRecover = false;

        // Act
        var result = MessageHelper.CreateVoidServiceFailed(requestId, voidQueueResults, isRecover);

        // Assert
        Assert.Contains($"Request {requestId}", result);
        Assert.Contains("failed to void", result);
        Assert.Contains("please contact IT for support", result);
        Assert.DoesNotContain("recover", result);
    }

    [Fact]
    public void CreateVoidServiceFailed_WithRecoverAction_ReturnsCorrectMessage()
    {
        // Arrange
        const int requestId = 456;
        var voidQueueResults = new List<TicketVoidingQueueModel>();
        const bool isRecover = true;

        // Act
        var result = MessageHelper.CreateVoidServiceFailed(requestId, voidQueueResults, isRecover);

        // Assert
        Assert.Contains($"Request {requestId}", result);
        Assert.Contains("failed to recover", result);
        Assert.Contains("please contact IT for support", result);
        Assert.DoesNotContain("void", result);
    }

    [Fact]
    public void CreateVoidServiceFailed_WithCricketFailedStatus_IncludesNetPositionMessage()
    {
        // Arrange
        const int requestId = 789;
        var voidQueueResults = new List<TicketVoidingQueueModel>
        {
            new()
            {
                VoidingTypeId = VoidingTypeConstant.Cricket,
                StatusId = ExecuteStatus.Failed
            }
        };
        const bool isRecover = false;

        // Act
        var result = MessageHelper.CreateVoidServiceFailed(requestId, voidQueueResults, isRecover);

        // Assert
        Assert.Contains($"Request {requestId}", result);
        Assert.Contains("failed to void", result);
        Assert.Contains("Failed to adjust Net Position of credit members", result);
        Assert.Contains("Consequently, their 'Match Winner' tickets have not been voided", result);
    }

    [Fact]
    public void CreateVoidServiceFailed_WithRecoverCricketFailedStatus_IncludesNetPositionMessage()
    {
        // Arrange
        const int requestId = 101;
        var voidQueueResults = new List<TicketVoidingQueueModel>
        {
            new()
            {
                VoidingTypeId = VoidingTypeConstant.RecoverCricket,
                StatusId = ExecuteStatus.Failed
            }
        };
        const bool isRecover = true;

        // Act
        var result = MessageHelper.CreateVoidServiceFailed(requestId, voidQueueResults, isRecover);

        // Assert
        Assert.Contains($"Request {requestId}", result);
        Assert.Contains("failed to recover", result);
        Assert.Contains("Failed to adjust Net Position of credit members", result);
        Assert.Contains("Consequently, their 'Match Winner' tickets have not been recovered", result);
    }

    [Fact]
    public void CreateVoidServiceFailed_WithCricketSuccessStatus_DoesNotIncludeNetPositionMessage()
    {
        // Arrange
        const int requestId = 202;
        var voidQueueResults = new List<TicketVoidingQueueModel>
        {
            new()
            {
                VoidingTypeId = VoidingTypeConstant.Cricket,
                StatusId = ExecuteStatus.Done
            }
        };
        const bool isRecover = false;

        // Act
        var result = MessageHelper.CreateVoidServiceFailed(requestId, voidQueueResults, isRecover);

        // Assert
        Assert.Contains($"Request {requestId}", result);
        Assert.Contains("failed to void", result);
        Assert.DoesNotContain("Failed to adjust Net Position of credit members", result);
        Assert.DoesNotContain("Consequently, their 'Match Winner' tickets have not been voided", result);
    }

    [Fact]
    public void CreateVoidServiceFailed_WithNonCricketFailedStatus_DoesNotIncludeNetPositionMessage()
    {
        // Arrange
        const int requestId = 303;
        List<TicketVoidingQueueModel> voidQueueResults =
        [
            new()
            {
                VoidingTypeId = VoidingTypeConstant.Single,
                StatusId = ExecuteStatus.Failed
            }
        ];
        const bool isRecover = false;

        // Act
        var result = MessageHelper.CreateVoidServiceFailed(requestId, voidQueueResults, isRecover);

        // Assert
        Assert.Contains($"Request {requestId}", result);
        Assert.Contains("failed to void", result);
        Assert.DoesNotContain("Failed to adjust Net Position of credit members", result);
        Assert.DoesNotContain("Consequently, their 'Match Winner' tickets have not been voided", result);
    }

    [Fact]
    public void CreateVoidServiceFailed_WithMixedStatuses_IncludesNetPositionMessage()
    {
        // Arrange
        const int requestId = 404;
        List<TicketVoidingQueueModel> voidQueueResults =
        [
            new()
            {
                VoidingTypeId = VoidingTypeConstant.Single,
                StatusId = ExecuteStatus.Done
            },

            new()
            {
                VoidingTypeId = VoidingTypeConstant.Cricket,
                StatusId = ExecuteStatus.Failed
            },

            new()
            {
                VoidingTypeId = VoidingTypeConstant.Parlay,
                StatusId = ExecuteStatus.Running
            }
        ];
        const bool isRecover = false;

        // Act
        var result = MessageHelper.CreateVoidServiceFailed(requestId, voidQueueResults, isRecover);

        // Assert
        Assert.Contains($"Request {requestId}", result);
        Assert.Contains("failed to void", result);
        Assert.Contains("Failed to adjust Net Position of credit members", result);
        Assert.Contains("Consequently, their 'Match Winner' tickets have not been voided", result);
    }

    [Fact]
    public void CreateVoidServiceFailed_WithEmptyVoidQueueResults_ReturnsBasicMessage()
    {
        // Arrange
        const int requestId = 505;
        var voidQueueResults = new List<TicketVoidingQueueModel>();
        const bool isRecover = false;

        // Act
        var result = MessageHelper.CreateVoidServiceFailed(requestId, voidQueueResults, isRecover);

        // Assert
        Assert.Contains($"Request {requestId}", result);
        Assert.Contains("failed to void", result);
        Assert.Contains("please contact IT for support", result);
        Assert.DoesNotContain("Failed to adjust Net Position of credit members", result);
    }

    [Fact]
    public void CreateVoidServiceFailed_WithMultipleCricketFailedStatuses_IncludesNetPositionMessage()
    {
        // Arrange
        const int requestId = 707;
        var voidQueueResults = new List<TicketVoidingQueueModel>
        {
            new()
            {
                VoidingTypeId = VoidingTypeConstant.Cricket,
                StatusId = ExecuteStatus.Failed
            },
            new()
            {
                VoidingTypeId = VoidingTypeConstant.RecoverCricket,
                StatusId = ExecuteStatus.Failed
            }
        };
        const bool isRecover = false;

        // Act
        var result = MessageHelper.CreateVoidServiceFailed(requestId, voidQueueResults, isRecover);

        // Assert
        Assert.Contains($"Request {requestId}", result);
        Assert.Contains("failed to void", result);
        Assert.Contains("Failed to adjust Net Position of credit members", result);
        Assert.Contains("Consequently, their 'Match Winner' tickets have not been voided", result);
    }

    [Fact]
    public void CreateVoidServiceFailed_WithRecoverActionAndCricketFailed_IncludesCorrectRecoverMessage()
    {
        // Arrange
        const int requestId = 808;
        var voidQueueResults = new List<TicketVoidingQueueModel>
        {
            new()
            {
                VoidingTypeId = VoidingTypeConstant.Cricket,
                StatusId = ExecuteStatus.Failed
            }
        };
        const bool isRecover = true;

        // Act
        var result = MessageHelper.CreateVoidServiceFailed(requestId, voidQueueResults, isRecover);

        // Assert
        Assert.Contains($"Request {requestId}", result);
        Assert.Contains("failed to recover", result);
        Assert.Contains("Failed to adjust Net Position of credit members", result);
        Assert.Contains("Consequently, their 'Match Winner' tickets have not been recovered", result);
    }

    [Fact]
    public void CreateVoidServiceFailed_WithZeroRequestId_ReturnsCorrectMessage()
    {
        // Arrange
        const int requestId = 0;
        var voidQueueResults = new List<TicketVoidingQueueModel>();
        const bool isRecover = false;

        // Act
        var result = MessageHelper.CreateVoidServiceFailed(requestId, voidQueueResults, isRecover);

        // Assert
        Assert.Contains("Request 0", result);
        Assert.Contains("failed to void", result);
        Assert.Contains("please contact IT for support", result);
    }

    [Fact]
    public void CreateVoidServiceFailed_WithNegativeRequestId_ReturnsCorrectMessage()
    {
        // Arrange
        const int requestId = -1;
        var voidQueueResults = new List<TicketVoidingQueueModel>();
        const bool isRecover = true;

        // Act
        var result = MessageHelper.CreateVoidServiceFailed(requestId, voidQueueResults, isRecover);

        // Assert
        Assert.Contains("Request -1", result);
        Assert.Contains("failed to recover", result);
        Assert.Contains("please contact IT for support", result);
    }
}