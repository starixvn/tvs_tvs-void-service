using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services.Void;

namespace TVS.Void.Service.Tests.Services.Void;

public class InsertAfterVoidDataPrivateServiceImpTests
{
    [Theory]
    [MemberData(nameof(GetMatchIdForAfterVoidShouldReturnZeroTestData))]
    public void GetMatchIdForAfterVoid_ShouldReturnZero(int issueTypeId, IEnumerable<RequestDetail> requestDetails, IEnumerable<TicketVoidingQueueModel> voidQueueResults)
    {
        // Arrange
        var service = new InsertAfterVoidDataPrivateServiceImp();

        // Act
        var actual = service.GetMatchIdForAfterVoid(issueTypeId, requestDetails, voidQueueResults);

        // Assert
        Assert.Equal(0, actual);
    }

    [Theory]
    [MemberData(nameof(GetMatchIdForAfterVoidShouldReturnValidMatchIdTestData))]
    public void GetMatchIdForAfterVoid_ShouldReturnValidMatchId(int issueTypeId, IEnumerable<RequestDetail> requestDetails, IList<TicketVoidingQueueModel> voidQueueResults)
    {
        // Arrange
        var service = new InsertAfterVoidDataPrivateServiceImp();

        // Act
        var actual = service.GetMatchIdForAfterVoid(issueTypeId, requestDetails, voidQueueResults);

        // Assert
        Assert.Equal(voidQueueResults.First().MatchId, actual);
    }

    [Theory]
    [MemberData(nameof(CombineNegativeBalanceUsersTestData))]
    public void CombineNegativeBalanceUsers_ShouldReturnExpectedData(IEnumerable<TicketVoidingQueueModel> voidQueueResults, string expectedResult)
    {
        // Arrange
        var service = new InsertAfterVoidDataPrivateServiceImp();

        // Act
        var actual = service.CombineNegativeBalanceUsers(voidQueueResults);

        // Assert
        Assert.Equal(expectedResult, actual);
    }

    public static IEnumerable<object[]> GetMatchIdForAfterVoidShouldReturnZeroTestData =>
        new List<object[]>
        {
            new object[] { IssueTypeConstant.AbnormalAccount, new List<RequestDetail> {new () {MatchId = 0}}, new List<TicketVoidingQueueModel>() },
            new object[] { IssueTypeConstant.AbnormalBet, new List<RequestDetail> {new () {MatchId = 29}, new () {MatchId = 1133}}, new List<TicketVoidingQueueModel>() },
            new object[] { IssueTypeConstant.DangerAccepted, new List<RequestDetail> (), new List<TicketVoidingQueueModel> {new () {MatchId = 113}, new() {MatchId = 223}} },
            new object[] { IssueTypeConstant.WrongScore, new List<RequestDetail> (), new List<TicketVoidingQueueModel> {new () {MatchId = 334}, new() {MatchId = 445}} }
        };

    public static IEnumerable<object[]> GetMatchIdForAfterVoidShouldReturnValidMatchIdTestData =>
        new List<object[]>
        {
            new object[] { IssueTypeConstant.AbnormalBet, new List<RequestDetail> {new () {MatchId = 1133 }, new () {MatchId = 1133}}, new List<TicketVoidingQueueModel>{new() {MatchId = 1133 } } },
            new object[] { IssueTypeConstant.DangerAccepted, new List<RequestDetail> (), new List<TicketVoidingQueueModel> {new () {MatchId = 223 }, new() {MatchId = 223}} },
            new object[] { IssueTypeConstant.WrongScore, new List<RequestDetail> (), new List<TicketVoidingQueueModel> {new () {MatchId = 334}, new() {MatchId = 334 } } }
        };

    public static IEnumerable<object[]> CombineNegativeBalanceUsersTestData =>
        new List<object[]>
        {
            new object[] {  new List<TicketVoidingQueueModel>{new() {NegativeBalanceUsers = "abc" } },  "abc"},
            new object[] {  new List<TicketVoidingQueueModel>
            {
                new() {NegativeBalanceUsers = "abc" }, new() {NegativeBalanceUsers = "xyz" }, new() { NegativeBalanceUsers = "xyz" }
            }, "abc,xyz"},
            new object[] {  new List<TicketVoidingQueueModel>
            {
                new() {NegativeBalanceUsers = "" }, new() {NegativeBalanceUsers = "xyz" }, new() { NegativeBalanceUsers = "" }, new() {NegativeBalanceUsers = "abc" }
            } , "xyz,abc"}
        };
}