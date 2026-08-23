using DirectoryService.Domain.Locations;
using Microsoft.Extensions.Logging;
using Moq;

namespace DirectoryService.UnitTests.Core.Locations;

internal static class LocationHandlerTestData
{
    public static Location CreateLocation(Guid id)
    {
        var name = LocationName.Create("Location Name").Value;
        var address = Address.Create("Country", "City", null, null).Value;
        return Location.Create(id, name, address, DateTime.UtcNow).Value;
    }

    public static void AssertStructuredProperty<T>(Mock<ILogger<T>> logger, string propertyName, object expectedValue)
    {
        var invocation = Assert.Single(logger.Invocations,
            x => string.Equals(x.Method.Name, nameof(ILogger.Log), StringComparison.Ordinal));
        var state = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object?>>>(invocation.Arguments[2]);
        Assert.Contains(state, property => string.Equals(property.Key, propertyName, StringComparison.Ordinal) &&
            object.Equals(property.Value, expectedValue));
    }
}
