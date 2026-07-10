using Microsoft.AspNetCore.Mvc;
using Tms.Api.Dtos;
using Tms.Api.Services;
namespace Tms.Api.Controllers;
[ApiController]
[Route("api/courses")]
public class CoursesController(ICourseService courseService) : ControllerBase
{
[HttpGet("{id:int}", Name = nameof(GetCourseById))]
public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
{
var course = await courseService.GetByIdAsync(id, ct);
return course is not null ? Ok(course) : NotFound();
}
[HttpPost]
[HttpPost]
public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
{
    // TODO 1: Call courseService.CodeExistsAsync(request.Code, ct).
    // If it returns true, return Conflict(new ProblemDetails { ... }) with:
    // Title = "Course code already exists"
    // Detail = $"A course with code '{request.Code}' is already registered."
    // Status = StatusCodes.Status409Conflict

    if (await courseService.CodeExistsAsync(request.Code, ct))
    {
        return Conflict(new ProblemDetails
        {
            Title = "Course code already exists",
            Detail = $"A course with code '{request.Code}' is already registered.",
            Status = StatusCodes.Status409Conflict
        });
    }

    // You do not need a try/catch; the framework's ProblemDetails middleware
    // handles unhandled exceptions.
    var result = await courseService.CreateAsync(request, ct);

    return CreatedAtAction(
        nameof(GetCourseById),
        new { id = result.Id },
        result);
}
}