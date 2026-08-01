using DirectoryService.Domain.Departments;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.UnitTests.Domain.Departments;

public class DepartmentSlugTests
{
    [Theory]
    [InlineData("sales")]
    [InlineData("b2b")]
    [InlineData("dept-1")]
    [InlineData("sub_dept_2")]
    [InlineData("a")]
    [InlineData("1")]
    [InlineData("a-b_c")]
    public void CreateValidSlugReturnsSuccess(string validSlug)
    {
        var result = DepartmentSlug.Create(validSlug);

        Assert.True(result.IsSuccess);
        Assert.Equal(validSlug, result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateNullOrWhitespaceSlugReturnsFailure(string? invalidSlug)
    {
#pragma warning disable CS8604
        var result = DepartmentSlug.Create(invalidSlug);
#pragma warning restore CS8604

        Assert.False(result.IsSuccess);
        Assert.Single(result.Error.Messages);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Theory]
    [InlineData("Sales")]
    [InlineData("sales!")]
    [InlineData("sa les")]
    [InlineData(".sales")]
    [InlineData("-sales")]
    [InlineData("_sales")]
    [InlineData("sales-")]
    [InlineData("sales_")]
    [InlineData("-")]
    [InlineData("_")]
    public void CreateInvalidSlugReturnsFailure(string invalidSlug)
    {
        var result = DepartmentSlug.Create(invalidSlug);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Error.Messages);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }
}