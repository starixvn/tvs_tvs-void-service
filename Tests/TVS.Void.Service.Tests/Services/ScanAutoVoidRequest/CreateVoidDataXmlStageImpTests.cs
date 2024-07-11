using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Tests.Services.ScanAutoVoidRequest;

public class CreateVoidDataXmlStageImpTests
{
    private readonly CreateVoidDataXmlStage createVoidDataXmlStage;

    public CreateVoidDataXmlStageImpTests()
    {
        createVoidDataXmlStage = new CreateVoidDataXmlStageImp();
    }

    private static DateTime CreateDateTimeFromString(string dateTime)
    {
        return DateTime.ParseExact(dateTime, "M/d/yyyy h:m:s tt", System.Globalization.CultureInfo.InvariantCulture);
    }

    [Fact]
    public void Execute_WhenIsAutoVoidIsTrue_ReturnsXmlStringOfVoid()
    {
        // Arrange
        const bool isAutoVoid = true;

        // Act
        var result = createVoidDataXmlStage.Execute(isAutoVoid, new CheckTicketValidationOutput());

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("VoidData CreatorName='starixIT' VoidFromTime", result);
        Assert.Contains("IsRunAutoAfterVoid='True'", result);
    }

    [Fact]
    public void Execute_WhenIsAutoVoidIsFalse_ReturnsXmlStringOfRecover()
    {
        // Arrange
        const bool isAutoVoid = false;
        var validation = new CheckTicketValidationOutput
        {
            RecoverSingleTickets = new List<RequestDetailTicketModel> { new() },
            RecoverParlayTickets = new List<RequestDetailTicketModel> { new() },
            RecoverCricketTickets = new List<RequestDetailTicketModel> { new() },
            RecoverSingleTickets14 = new List<RequestDetailTicketModel> { new() },
            RecoverParlayTickets14 = new List<RequestDetailTicketModel> { new() }
        };

        // Act
        var result = createVoidDataXmlStage.Execute(isAutoVoid, validation);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("VoidData CreatorName='starixIT' IsRunAutoAfterVoid='True' TotalTicket='5'", result);
    }

    [Fact]
    public void ExecuteForAutoVoid_WhenVoidTicketsExist_ReturnsXmlStringWithIsHasSettledTicket()
    {
        // Arrange
        const bool isAutoVoid = true;
        var validation = new CheckTicketValidationOutput
        {
            VoidSingleTickets = new List<RequestDetailTicketModel>
            {
                new() { StatusId = TicketStatus.WonId },
                new() { StatusId = TicketStatus.LoseId },
                new() { StatusId = TicketStatus.DrawId }
            }
        };

        // Act
        var result = createVoidDataXmlStage.Execute(isAutoVoid, validation);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("VoidData", result);
        Assert.Contains("TotalTicket='3'", result);
        Assert.Contains("IsHasSettledTicket='True'", result);
    }

    [Fact]
    public void ExecuteForAutoVoid_WhenOnlyVoidTicketsExist_ReturnsXmlStringWithMinTimeOfVoidTickets()
    {
        // Arrange
        const bool isAutoVoid = true;
        var validation = new CheckTicketValidationOutput
        {
            VoidSingleTickets = new List<RequestDetailTicketModel>
            {
                new (){ StatusId = TicketStatus.WonId, TransDate = CreateDateTimeFromString("7/1/2024 5:14:54 PM") , WinLostDate =  CreateDateTimeFromString("7/7/2024 4:14:54 PM") }
            },
            VoidParlayTickets = new List<RequestDetailTicketModel>
            {
                new () { StatusId = TicketStatus.LoseId, TransDate =  CreateDateTimeFromString("7/4/2024 3:14:54 PM"), WinLostDate =  CreateDateTimeFromString("7/20/2024 4:14:54 PM") }
            },
            CancelCOTickets = new List<RequestDetailTicketModel>(),
            VoidCricketTickets = new List<RequestDetailTicketModel>(),
            VoidSingleTickets14 = new List<RequestDetailTicketModel>(),
            VoidParlayTickets14 = new List<RequestDetailTicketModel>()
        };

        // Act
        var result = createVoidDataXmlStage.Execute(isAutoVoid, validation);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("VoidFromTime='07/02/2024 05:14:54 AM'", result);
        Assert.Contains("VoidToTime='07/05/2024 03:14:54 AM'", result);
        Assert.Contains("VoidFromDate='07/08/2024'", result);
        Assert.Contains("VoidToDate='07/21/2024'", result);
        Assert.Contains("TotalTicket='2'", result);
    }

    [Fact]
    public void ExecuteForAutoVoid_WhenVoidTicketsAndCancelCashOutTicketsExist_ReturnsXmlStringWithTimeOfTwoSets()
    {
        // Arrange
        const bool isAutoVoid = true;
        var validation = new CheckTicketValidationOutput
        {
            VoidSingleTickets = new List<RequestDetailTicketModel>
            {
                new (){ StatusId = TicketStatus.WonId, TransDate = CreateDateTimeFromString("7/5/2024 5:14:54 PM") , WinLostDate =  CreateDateTimeFromString("7/7/2024 4:14:54 PM") }
            },
            VoidParlayTickets = new List<RequestDetailTicketModel>
            {
                new () { StatusId = TicketStatus.LoseId, TransDate =  CreateDateTimeFromString("7/9/2024 6:14:54 PM"), WinLostDate =  CreateDateTimeFromString("7/20/2024 4:14:54 PM") }
            },
            CancelCOTickets = new List<RequestDetailTicketModel>
            {
                new () { StatusId = TicketStatus.RunningId,CheckTimeCancelCashOut =  CreateDateTimeFromString("7/2/2024 3:14:54 PM")},
                new () { StatusId = TicketStatus.RunningId,CheckTimeCancelCashOut =  CreateDateTimeFromString("7/28/2024 3:14:54 PM")}
            },
            VoidCricketTickets = new List<RequestDetailTicketModel>(),
            VoidSingleTickets14 = new List<RequestDetailTicketModel>(),
            VoidParlayTickets14 = new List<RequestDetailTicketModel>()
        };

        // Act
        var result = createVoidDataXmlStage.Execute(isAutoVoid, validation);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("VoidFromTime='07/03/2024 03:14:54 AM'", result);
        Assert.Contains("VoidToTime='07/29/2024 03:14:54 AM'", result);
        Assert.Contains("VoidFromDate='07/08/2024'", result);
        Assert.Contains("VoidToDate='07/29/2024'", result);
        Assert.Contains("TotalTicket='4'", result);
    }

    [Fact]
    public void ExecuteForAutoVoid_WhenOnlyCancelCashOutTicketsExist_ReturnsXmlStringWithTimeOfCancelCashOut()
    {
        // Arrange
        const bool isAutoVoid = true;
        var validation = new CheckTicketValidationOutput
        {
            VoidSingleTickets = new List<RequestDetailTicketModel>(),
            VoidParlayTickets = new List<RequestDetailTicketModel>(),
            CancelCOTickets = new List<RequestDetailTicketModel>
            {
                new () { StatusId = TicketStatus.RunningId,CheckTimeCancelCashOut =  CreateDateTimeFromString("7/4/2024 3:14:54 PM")},
                new () { StatusId = TicketStatus.RunningId,CheckTimeCancelCashOut =  CreateDateTimeFromString("7/9/2024 3:14:54 PM")}
            },
            VoidCricketTickets = new List<RequestDetailTicketModel>(),
            VoidSingleTickets14 = new List<RequestDetailTicketModel>(),
            VoidParlayTickets14 = new List<RequestDetailTicketModel>()
        };

        // Act
        var result = createVoidDataXmlStage.Execute(isAutoVoid, validation);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("VoidFromTime='07/05/2024 03:14:54 AM'", result);
        Assert.Contains("VoidToTime='07/10/2024 03:14:54 AM'", result);
        Assert.Contains("VoidFromDate='07/05/2024'", result);
        Assert.Contains("VoidToDate='07/10/2024'", result);
        Assert.Contains("TotalTicket='2'", result);
    }
}