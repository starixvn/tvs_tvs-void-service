using NSubstitute;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Tests.Services.Models;

public class RequestModelTests
{
    private readonly RequestDetailTicketModelEqualityComparer service;

    public RequestModelTests()
    {
        service = new RequestDetailTicketModelEqualityComparer();
    }

    [Fact]
    public void Equals_ModelHasValue_ReturnTrue()
    {
        // Arrange
        var model1 = new RequestDetailTicketModel();
        var model2 = new RequestDetailTicketModel();

        // Act
        var result = service.Equals(model1, model2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_ModelHasValue_ReturnFalse()
    {
        // Arrange
        var model1 = new RequestDetailTicketModel { RequestId = 1 };
        var model2 = new RequestDetailTicketModel();

        // Act
        var result = service.Equals(model1, model2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_ModelOneIsNull_ReturnFalse()
    {
        // Arrange
        var model2 = new RequestDetailTicketModel();

        // Act
        var result = service.Equals(null, model2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_ModelsIsNull_ReturnTrue()
    {
        // Arrange
        // Act
        var result = service.Equals(null, null);

        // Assert
        Assert.True(result);
    }


    [Fact]
    public void Equals_ModelsHasDifferentType_ReturnFalse()
    {
        // Arrange
        var model1 = new RequestDetailTicketModel();
        var model2 = Substitute.For<RequestDetailTicketModel>();

        // Act
        var result = service.Equals(model1, model2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_ForEqualObjects_ReturnSameHash()
    {
        // Arrange
        var model1 = new RequestDetailTicketModel
        {
            RequestId = 1,
            TransId = 1,
            MatchId = 2,
            MainTransId = 4,
            SiteDbGroupId = 2,
            IssueTypeId = 3
        };

        var model2 = new RequestDetailTicketModel
        {
            RequestId = 1,
            TransId = 1,
            MatchId = 2,
            MainTransId = 4,
            SiteDbGroupId = 2,
            IssueTypeId = 3
        };

        // Act
        var hashCode1 = service.GetHashCode(model1);
        var hashCode2 = service.GetHashCode(model2);

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }
}