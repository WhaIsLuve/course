using DirectoryService.Domain.Positions;

namespace DirectoryService.UnitTests.Core.Positions;

internal static class PositionHandlerTestData
{
    public static Position CreatePosition(Guid id)
    {
        var name = PositionName.Create("Developer").Value;
        return Position.Create(id, name, DateTime.UtcNow.AddMinutes(-1)).Value;
    }
}
