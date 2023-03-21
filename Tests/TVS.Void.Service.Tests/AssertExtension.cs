using System.Text.Json;

namespace TVS.Void.Service.Tests;

public static class AssertExtension
{
    public static void Equal(object expected, object actual)
    {
        var expectedJson = JsonSerializer.Serialize(expected);
        var actualJson = JsonSerializer.Serialize(actual);

        Assert.Equal(expectedJson, actualJson);
    }
}