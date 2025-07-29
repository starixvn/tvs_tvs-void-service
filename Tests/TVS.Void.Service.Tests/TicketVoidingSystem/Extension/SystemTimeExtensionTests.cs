using TicketVoidingSystem.Extension;

namespace TVS.Void.Service.Tests.TicketVoidingSystem.Extension;

public class SystemTimeExtensionTests
{
    [Fact]
    public void ZoneManila_WithUtcDateTime_ShouldAdd12Hours()
    {
        // Arrange
        var utcDateTime = new DateTime(2024, 1, 1, 10, 30, 0, DateTimeKind.Utc);
        var expectedDateTime = new DateTime(2024, 1, 1, 22, 30, 0, DateTimeKind.Utc);

        // Act
        var actual = utcDateTime.ZoneManila();

        // Assert
        Assert.Equal(expectedDateTime, actual);
    }

    [Fact]
    public void ZoneManila_WithLocalDateTime_ShouldAdd12Hours()
    {
        // Arrange
        var localDateTime = new DateTime(2024, 1, 1, 8, 15, 0, DateTimeKind.Local);
        var expectedDateTime = new DateTime(2024, 1, 1, 20, 15, 0, DateTimeKind.Local);

        // Act
        var actual = localDateTime.ZoneManila();

        // Assert
        Assert.Equal(expectedDateTime, actual);
    }

    [Fact]
    public void ZoneManila_WithMidnightTime_ShouldAdd12Hours()
    {
        // Arrange
        var midnightDateTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var expectedDateTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var actual = midnightDateTime.ZoneManila();

        // Assert
        Assert.Equal(expectedDateTime, actual);
    }

    [Fact]
    public void ZoneManila_WithDateTimeNearMidnight_ShouldAdd12Hours()
    {
        // Arrange
        var nearMidnightDateTime = new DateTime(2024, 1, 1, 23, 59, 59, DateTimeKind.Utc);
        var expectedDateTime = new DateTime(2024, 1, 2, 11, 59, 59, DateTimeKind.Utc);

        // Act
        var actual = nearMidnightDateTime.ZoneManila();

        // Assert
        Assert.Equal(expectedDateTime, actual);
    }

    [Fact]
    public void ZoneManila_WithDateTimeNearNoon_ShouldAdd12Hours()
    {
        // Arrange
        var nearNoonDateTime = new DateTime(2024, 1, 1, 11, 30, 45, DateTimeKind.Utc);
        var expectedDateTime = new DateTime(2024, 1, 1, 23, 30, 45, DateTimeKind.Utc);

        // Act
        var actual = nearNoonDateTime.ZoneManila();

        // Assert
        Assert.Equal(expectedDateTime, actual);
    }

    [Fact]
    public void ZoneManila_WithLeapYearDate_ShouldAdd12Hours()
    {
        // Arrange
        var leapYearDateTime = new DateTime(2024, 2, 29, 10, 0, 0, DateTimeKind.Utc);
        var expectedDateTime = new DateTime(2024, 2, 29, 22, 0, 0, DateTimeKind.Utc);

        // Act
        var actual = leapYearDateTime.ZoneManila();

        // Assert
        Assert.Equal(expectedDateTime, actual);
    }

    [Fact]
    public void ZoneManila_WithMinDateTime_ShouldAdd12Hours()
    {
        // Arrange
        var minDateTime = DateTime.MinValue;
        var expectedDateTime = DateTime.MinValue.AddHours(12);

        // Act
        var actual = minDateTime.ZoneManila();

        // Assert
        Assert.Equal(expectedDateTime, actual);
    }
}