using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Departments;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Web.Controllers;

[ApiController]
[Route("api/v1/departments")]
#pragma warning disable CA1515
public sealed class DepartmentController(IDepartmentService departmentService) : ControllerBase
#pragma warning restore CA1515
{
    private readonly IDepartmentService _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));

    [HttpPost]
    public async Task<IResult> Create([FromBody] CreateDepartmentDto dto, CancellationToken cancellationToken)
    {
        var id = await _departmentService.CreateAsync(dto, cancellationToken);
        HttpContext.Response.Headers.Append("Location", id.ToString());
        return TypedResults.Created();
    }

    [HttpGet]
    public async Task<IResult> Get(CancellationToken cancellationToken)
    {
        return TypedResults.Ok();
    }

    [HttpGet("{id:guid}")]
    public async Task<IResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        return TypedResults.Ok();
    }

    [HttpPut("{id:guid}")]
    public async Task<IResult> Update([FromRoute] Guid id, [FromBody] UpdateDepartmentDto dto, CancellationToken cancellationToken)
    {
        return TypedResults.Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        return TypedResults.Ok();
    }

    [HttpPut("{id:guid}/location")]
    public async Task<IResult> AddLocation([FromRoute] Guid id, [FromQuery] Guid locationId, CancellationToken cancellationToken)
    {
        return TypedResults.Ok();
    }
    
    [HttpPut("{id:guid}/position")]
    public async Task<IResult> AddPosition([FromRoute] Guid id, [FromQuery] Guid positionId, CancellationToken cancellationToken)
    {
        return TypedResults.Ok();
    }
}