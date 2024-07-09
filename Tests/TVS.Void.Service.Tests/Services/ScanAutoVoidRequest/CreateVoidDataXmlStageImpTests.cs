using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Tests.Services.ScanAutoVoidRequest;

public class CreateVoidDataXmlStageImpTests
{
    private readonly CheckTicketValidationOutput validation;
    private readonly CreateVoidDataXmlStage createVoidDataXmlStage;

    public CreateVoidDataXmlStageImpTests()
    {
        validation = new CheckTicketValidationOutput();
        createVoidDataXmlStage = new CreateVoidDataXmlStageImp();
    }

    [Fact]
    public void ExecuteForAutoVoid_WhenIsAutoVoidIsTrue_ReturnsXmlString()
    {
        // Arrange
        const bool isAutoVoid = true;

        // Act
        var result = createVoidDataXmlStage.ExecuteForAutoVoid(isAutoVoid, validation);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("VoidData", result);
        Assert.Contains("CreatorName='starixIT'", result);
        Assert.Contains("IsRunAutoAfterVoid='True'", result);
    }

    [Fact]
    public void ExecuteForAutoVoid_WhenIsAutoVoidIsFalse_ReturnsEmptyString()
    {
        // Arrange
        var isAutoVoid = false;

        // Act
        var result = createVoidDataXmlStage.ExecuteForAutoVoid(isAutoVoid, validation);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ExecuteForAutoVoid_WhenVoidTicketsExist_ReturnsXmlStringWithMinTimeOfVoidTickets()
    {
        // Arrange
        var isAutoVoid = true;
        var voidSingleTickets = new List<RequestDetailTicketModel>
                {
                    new ()
                    {
                        StatusId = TicketStatus.WonId, TransDate = DateTime.ParseExact("7/1/2024 5:14:54 PM", "M/d/yyyy h:m:s tt", System.Globalization.CultureInfo.InvariantCulture),
                        WinLostDate = DateTime.ParseExact("7/2/2024 5:14:54 PM", "M/d/yyyy h:m:s tt", System.Globalization.CultureInfo.InvariantCulture)
                    }
                };
        var voidParlayTickets = new List<RequestDetailTicketModel>
                {
                    new ()
                    {
                        StatusId = TicketStatus.LoseId, TransDate = DateTime.ParseExact("7/4/2024 3:14:54 PM", "M/d/yyyy h:m:s tt", System.Globalization.CultureInfo.InvariantCulture),
                        WinLostDate = DateTime.ParseExact("7/2/2024 5:14:54 PM", "M/d/yyyy h:m:s tt", System.Globalization.CultureInfo.InvariantCulture)
                    }
                };
        var cancelCashOutTickets = new List<RequestDetailTicketModel>
        {
            new () { StatusId = TicketStatus.RunningId, CheckTimeCancelCashOut = DateTime.ParseExact("7/3/2024 6:14:54 PM", "M/d/yyyy h:m:s tt", System.Globalization.CultureInfo.InvariantCulture)}
        };
        var voidCricketTickets = new List<RequestDetailTicketModel>();
        var voidSingleTickets14 = new List<RequestDetailTicketModel>();
        var voidParlayTickets14 = new List<RequestDetailTicketModel>();
        validation.VoidSingleTickets = voidSingleTickets;
        validation.VoidParlayTickets = voidParlayTickets;
        validation.CancelCOTickets = cancelCashOutTickets;
        validation.VoidCricketTickets = voidCricketTickets;
        validation.VoidSingleTickets14 = voidSingleTickets14;
        validation.VoidParlayTickets14 = voidParlayTickets14;

        // Act
        var result = createVoidDataXmlStage.ExecuteForAutoVoid(isAutoVoid, validation);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("VoidData", result);
        Assert.Contains("VoidFromTIme='xxxx'", result);
        Assert.Contains("TotalTicket='3'", result);
        Assert.Contains("IsHasSettledTicket='True'", result);
    }

    [Fact]
    public void ExecuteForAutoVoid_WhenVoidTicketsDoNotExist_ReturnsXmlStringWithoutVoidTicketData()
    {
        // Arrange
        var isAutoVoid = true;
        var voidSingleTickets = new List<RequestDetailTicketModel>();
        var voidParlayTickets = new List<RequestDetailTicketModel>();
        var voidCricketTickets = new List<RequestDetailTicketModel>();
        var voidSingleTickets14 = new List<RequestDetailTicketModel>();
        var voidParlayTickets14 = new List<RequestDetailTicketModel>();
        validation.VoidSingleTickets = voidSingleTickets;
        validation.VoidParlayTickets = voidParlayTickets;
        validation.VoidCricketTickets = voidCricketTickets;
        validation.VoidSingleTickets14 = voidSingleTickets14;
        validation.VoidParlayTickets14 = voidParlayTickets14;

        // Act
        var result = createVoidDataXmlStage.ExecuteForAutoVoid(isAutoVoid, validation);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("VoidData", result);
        Assert.Contains("TotalTicket='0'", result);
        Assert.Contains("IsHasSettledTicket='False'", result);
    }

    [Fact]
    public void ExecuteForAutoVoid_WhenCancelCashOutTicketsExist_ReturnsXmlStringWithCancelCashOutTicketData()
    {
        // Arrange
        var isAutoVoid = true;
        var cancelCashOutTickets = new List<RequestDetailTicketModel>
                {
                    new RequestDetailTicketModel { StatusId = TicketStatus.WonId },
                    new RequestDetailTicketModel { StatusId = TicketStatus.DrawId }
                };
        validation.CancelCOTickets = cancelCashOutTickets;

        // Act
        var result = createVoidDataXmlStage.ExecuteForAutoVoid(isAutoVoid, validation);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("VoidData", result);
        Assert.Contains("TotalTicket='2'", result);
        Assert.Contains("IsHasSettledTicket='True'", result);
    }

    [Fact]
    public void ExecuteForAutoVoid_WhenCancelCashOutTicketsDoNotExist_ReturnsXmlStringWithoutCancelCashOutTicketData()
    {
        // Arrange
        var isAutoVoid = true;
        var cancelCashOutTickets = new List<RequestDetailTicketModel>();
        validation.CancelCOTickets = cancelCashOutTickets;

        // Act
        var result = createVoidDataXmlStage.ExecuteForAutoVoid(isAutoVoid, validation);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("VoidData", result);
        Assert.Contains("TotalTicket='0'", result);
        Assert.Contains("IsHasSettledTicket='False'", result);
    }
}