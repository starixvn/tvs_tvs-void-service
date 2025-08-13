using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Models;
using TicketVoidingSystem.Utilities;

namespace TVS.Void.Service.Tests.TicketVoidingSystem.Utilities;

public class HelperTests
{
    [Fact]
    public void GetSettingKeyBySiteDbGroupId_WithValidSiteDbGroupId_ShouldReturnExpectedKey()
    {
        // Arrange
        const int siteDbGroupId = 1;
        const string spName = "TestSpName";
        const string expectedServerName = "DBSITE01";
        const string expectedKey = $"{expectedServerName}.{spName}";

        // Setup server list
        var serverList = new List<SiteDbGroupModel>
            {
                new() { Id = 0, ServerName = "DBCTG" },
                new() { Id = 1, ServerName = "DBSITE01" },
                new() { Id = 2, ServerName = "DBSITE02" },
                new() { Id = 3, ServerName = "DBSITE03" }
            };
        SiteDbGroupConstant.SetServerList(serverList);

        // Act
        var actual = Helper.GetSettingKeyBySiteDbGroupId(siteDbGroupId, spName);

        // Assert
        Assert.Equal(expectedKey, actual);
    }

    [Fact]
    public void GetSettingKeyBySiteDbGroupId_WithCentralDb_ShouldReturnExpectedKey()
    {
        // Arrange
        const int siteDbGroupId = 0;
        const string spName = "CentralSpName";
        const string expectedServerName = "DBCTG";
        const string expectedKey = $"{expectedServerName}.{spName}";

        // Setup server list
        var serverList = new List<SiteDbGroupModel>
            {
                new() { Id = 0, ServerName = "DBCTG" },
                new() { Id = 1, ServerName = "DBSITE01" },
                new() { Id = 2, ServerName = "DBSITE02" },
                new() { Id = 3, ServerName = "DBSITE03" }
            };
        SiteDbGroupConstant.SetServerList(serverList);

        // Act
        var actual = Helper.GetSettingKeyBySiteDbGroupId(siteDbGroupId, spName);

        // Assert
        Assert.Equal(expectedKey, actual);
    }

    [Fact]
    public void GetSettingKeyBySiteDbGroupId_WithSiteDb3_ShouldReturnExpectedKey()
    {
        // Arrange
        const int siteDbGroupId = 3;
        const string spName = "Site3SpName";
        const string expectedServerName = "DBSITE03";
        const string expectedKey = $"{expectedServerName}.{spName}";

        // Setup server list
        var serverList = new List<SiteDbGroupModel>
            {
                new() { Id = 0, ServerName = "DBCTG" },
                new() { Id = 1, ServerName = "DBSITE01" },
                new() { Id = 2, ServerName = "DBSITE02" },
                new() { Id = 3, ServerName = "DBSITE03" }
            };
        SiteDbGroupConstant.SetServerList(serverList);

        // Act
        var actual = Helper.GetSettingKeyBySiteDbGroupId(siteDbGroupId, spName);

        // Assert
        Assert.Equal(expectedKey, actual);
    }

    [Fact]
    public void GetSettingKeyBySiteDbGroupId_WithEmptySpName_ShouldReturnExpectedKey()
    {
        // Arrange
        const int siteDbGroupId = 2;
        const string spName = "";
        var expectedServerName = "DBSITE02";
        var expectedKey = $"{expectedServerName}.{spName}";

        // Setup server list
        var serverList = new List<SiteDbGroupModel>
            {
                new() { Id = 0, ServerName = "DBCTG" },
                new() { Id = 1, ServerName = "DBSITE01" },
                new() { Id = 2, ServerName = "DBSITE02" },
                new() { Id = 3, ServerName = "DBSITE03" }
            };
        SiteDbGroupConstant.SetServerList(serverList);

        // Act
        var actual = Helper.GetSettingKeyBySiteDbGroupId(siteDbGroupId, spName);

        // Assert
        Assert.Equal(expectedKey, actual);
    }
}