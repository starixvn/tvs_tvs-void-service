using NSubstitute;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Tests.Services;

public class AfterVoidServiceModelTests
{
    private readonly AfterVoidServiceModelEqualComparer service = new();

    [Fact]
    public void Equals_ModelHasValue_ReturnTrue()
    {
        // Arrange
        var x = new AfterVoidServiceModel();
        var y = new AfterVoidServiceModel();

        // Act
        var result = service.Equals(x, y);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_ModelHasValue_ReturnFalse()
    {
        // Arrange
        var x = new AfterVoidServiceModel { SessionId = 1 };
        var y = new AfterVoidServiceModel();

        // Act
        var result = service.Equals(x, y);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_ModelOneIsNull_ReturnFalse()
    {
        // Arrange
        var model2 = new AfterVoidServiceModel();

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
        var model1 = new AfterVoidServiceModel();
        var model2 = Substitute.For<AfterVoidServiceModel>();

        // Act
        var result = service.Equals(model1, model2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_ForEqualObjects_ReturnSameHash()
    {
        // Arrange
        var model1 = new AfterVoidServiceModel
        {
            SessionId = 1,
            MatchId = 2,
            RequestId = 3,
            VoidingHistoryId = 4,
            StatusId = 5,
            NegativeBalanceUsers = "user1"
        };

        var model2 = new AfterVoidServiceModel
        {
            SessionId = 1,
            MatchId = 2,
            RequestId = 3,
            VoidingHistoryId = 4,
            StatusId = 5,
            NegativeBalanceUsers = "user1"
        };

        // Act
        var hashCode1 = service.GetHashCode(model1);
        var hashCode2 = service.GetHashCode(model2);

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }
}