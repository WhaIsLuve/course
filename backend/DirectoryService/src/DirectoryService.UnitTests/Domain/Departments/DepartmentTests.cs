using DirectoryService.Domain.Departments;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.UnitTests.Domain.Departments;

public class DepartmentTests
{
    private readonly DepartmentName _validName = DepartmentName.Create("Sales").Value;
    private readonly DepartmentSlug _validSlug = DepartmentSlug.Create("sales").Value;
    private readonly DepartmentPath _validPath = DepartmentPath.Create("/sales").Value;
    private readonly DateTime _validDate = DateTime.UtcNow;

    [Fact]
    public void CreateRootDepartmentReturnsSuccess()
    {
        var result = Department.Create(Guid.NewGuid(), _validName, _validSlug, null, _validDate);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.ParentId);
        Assert.Equal("/sales", result.Value.Path.Value);
    }

    [Fact]
    public void CreateChildDepartmentReturnsSuccess()
    {
        var parentId = Guid.NewGuid();
        var parentInfo = new ParentInfo(parentId, _validPath);
        var result = Department.Create(Guid.NewGuid(), _validName, _validSlug, parentInfo, _validDate);

        Assert.True(result.IsSuccess);
        Assert.Equal(parentId, result.Value.ParentId);
        Assert.Equal("/sales/sales", result.Value.Path.Value);
    }

    [Fact]
    public void CreateEmptyIdReturnsFailure()
    {
        var result = Department.Create(Guid.Empty, _validName, _validSlug, null, _validDate);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Error.Messages);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void CreateEmptyParentIdReturnsFailure()
    {
        var parentInfo = new ParentInfo(Guid.Empty, _validPath);
        var result = Department.Create(Guid.NewGuid(), _validName, _validSlug, parentInfo, _validDate);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Error.Messages);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void CreateDefaultCreatedAtReturnsFailure()
    {
        var result = Department.Create(Guid.NewGuid(), _validName, _validSlug, null, default);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Error.Messages);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void UpdateValidDataReturnsSuccess()
    {
        var department = Department.Create(Guid.NewGuid(), _validName, _validSlug, null, _validDate).Value;
        var newName = DepartmentName.Create("New Sales").Value;

        var result = department.Update(newName, null, _validDate.AddHours(1));

        Assert.True(result.IsSuccess);
        Assert.Equal("New Sales", department.Name.Value);
        Assert.Equal(_validDate.AddHours(1), department.UpdatedAt);
    }

    [Fact]
    public void UpdateChangeParentReturnsSuccess()
    {
        var department = Department.Create(Guid.NewGuid(), _validName, _validSlug, null, _validDate).Value;
        var newParentId = Guid.NewGuid();
        var newParentInfo = new ParentInfo(newParentId, DepartmentPath.Create("/company").Value);

        var result = department.Update(_validName, newParentInfo, _validDate.AddHours(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(newParentId, department.ParentId);
        Assert.Equal("/company/sales", department.Path.Value);
    }

    [Fact]
    public void UpdateDefaultUpdatedAtReturnsFailure()
    {
        var department = Department.Create(Guid.NewGuid(), _validName, _validSlug, null, _validDate).Value;

        var result = department.Update(_validName, null, default);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Error.Messages);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void UpdateUpdatedAtLessThanCreatedAtReturnsFailure()
    {
        var department = Department.Create(Guid.NewGuid(), _validName, _validSlug, null, _validDate).Value;

        var result = department.Update(_validName, null, _validDate.AddHours(-1));

        Assert.False(result.IsSuccess);
        Assert.Single(result.Error.Messages);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void UpdateUpdatedAtEqualsCreatedAtReturnsFailure()
    {
        var department = Department.Create(Guid.NewGuid(), _validName, _validSlug, null, _validDate).Value;

        var result = department.Update(_validName, null, _validDate);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Error.Messages);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void UpdateEmptyParentIdReturnsFailure()
    {
        var department = Department.Create(Guid.NewGuid(), _validName, _validSlug, null, _validDate).Value;
        var emptyParentInfo = new ParentInfo(Guid.Empty, _validPath);

        var result = department.Update(_validName, emptyParentInfo, _validDate.AddHours(1));

        Assert.False(result.IsSuccess);
        Assert.Single(result.Error.Messages);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }
}