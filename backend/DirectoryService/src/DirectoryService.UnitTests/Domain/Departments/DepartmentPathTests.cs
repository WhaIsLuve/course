using DirectoryService.Domain.Departments;

namespace DirectoryService.UnitTests.Domain.Departments;

public class DepartmentPathTests
{
    [Theory]
    [InlineData("/sales")]
    [InlineData("/company/sales/b2b")]
    [InlineData("/dept-1/sub_dept_2")]
    [InlineData("/a")]
    [InlineData("/1")]
    [InlineData("/a-b_c")]
    public void CreateValidPathReturnsSuccess(string validPath)
    {
        var result = DepartmentPath.Create(validPath);

        Assert.True(result.IsSuccess);
        Assert.Equal(validPath, result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateNullOrWhitespacePathReturnsFailure(string? invalidPath)
    {
#pragma warning disable CS8604
        var result = DepartmentPath.Create(invalidPath);
#pragma warning restore CS8604

        Assert.False(result.IsSuccess);
        Assert.Equal("Путь пустой", result.Error.GetMessage());
    }

    [Theory]
    [InlineData("sales")]
    [InlineData("company/sales")]
    [InlineData(" sales")]
    public void CreatePathNotStartingWithSlashReturnsFailure(string invalidPath)
    {
        var result = DepartmentPath.Create(invalidPath);

        Assert.False(result.IsSuccess);
        Assert.Equal("Путь должен начинаться с /", result.Error.GetMessage());
    }

    [Theory]
    [InlineData("//sales")]
    [InlineData("/company//sales")]
    [InlineData("/sales//")]
    public void CreatePathWithDoubleSlashReturnsFailure(string invalidPath)
    {
        var result = DepartmentPath.Create(invalidPath);

        Assert.False(result.IsSuccess);
        Assert.Equal("Путь не может содержать //", result.Error.GetMessage());
    }

    [Theory]
    [InlineData("/sales/")]
    public void CreatePathWithEmptySegmentReturnsFailure(string invalidPath)
    {
        var result = DepartmentPath.Create(invalidPath);

        Assert.False(result.IsSuccess);
        Assert.Equal("Путь не может содержать пустые сегменты", result.Error.GetMessage());
    }

    [Theory]
    [InlineData("/Sales")]
    [InlineData("/sales!")]
    [InlineData("/-sales")]
    [InlineData("/sales-")]
    [InlineData("/_sales")]
    [InlineData("/sales_")]
    [InlineData("/-")]
    [InlineData("/_")]
    [InlineData("/sa les")]
    [InlineData("/.sales")]
    public void CreateInvalidSegmentFormatReturnsFailure(string invalidPath)
    {
        var result = DepartmentPath.Create(invalidPath);

        Assert.False(result.IsSuccess);
        Assert.Contains("Путь может содержать только символы в нижнем регистре и цифры", result.Error.GetMessage(),
            StringComparison.OrdinalIgnoreCase);
    }
}