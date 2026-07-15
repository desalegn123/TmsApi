using Microsoft.AspNetCore.Mvc;
using Tms.Api.Dtos;
using Tms.Api.Services;

namespace Tms.Api.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
[Tags("Enrollments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class EnrollmentsController(
    ICourseService courseService,
    IEnrollmentService enrollmentService) : ControllerBase
{
    // NEW ACTION (Step 6)
   [HttpGet(Name = "ListCourseEnrollments")]
[ProducesResponseType(typeof(IReadOnlyList<EnrollmentResponseDto>),
StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[EndpointSummary("List enrolments for a course")]
    public async Task<IActionResult> GetEnrollments(
        int courseId,
        CancellationToken ct)
    {
        // TODO 4: Confirm the course exists
        var course = await courseService.GetByIdAsync(courseId, ct);

        if (course is null)
        {
            return NotFound();
        }

        // Return all enrollments for the course
        var enrollments = await enrollmentService.GetByCourseAsync(courseId, ct);

        return Ok(enrollments);
    }

   [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
[ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[EndpointSummary("Get one enrolment for a course")]    public async Task<IActionResult> GetEnrollment(
        int courseId,
        int id,
        CancellationToken ct)
    {
        var enrollment = await enrollmentService.GetByIdAsync(courseId, id, ct);

        return enrollment is not null
            ? Ok(enrollment)
            : NotFound();
    }

   [HttpPost]
[ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.
Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
[EndpointSummary("Enrol a student in a course")]
[EndpointDescription("Returns 404 if the course does not exist, 409if the course has reached MaxCapacity.")]
    public async Task<IActionResult> EnrollStudent(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        // Step 1: Check whether the course exists (404 first)
        var course = await courseService.GetByIdAsync(courseId, ct);

        if (course is null)
        {
            return NotFound();
        }

        // Step 2: Check whether the course is full (409)
        if (course.EnrollmentCount >= course.MaxCapacity)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course is full",
                Detail = $"Course '{course.Title}' has reached its maximum capacity of {course.MaxCapacity}.",
                Status = StatusCodes.Status409Conflict
            });
        }

        // Step 3: Create the enrollment
        var enrollment = await enrollmentService.CreateAsync(courseId, request, ct);

        // Step 4: Return 201 Created
        return CreatedAtAction(
            nameof(GetEnrollment),
            new { courseId, id = enrollment.Id },
            enrollment);
    }
}