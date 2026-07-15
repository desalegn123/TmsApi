using Microsoft.EntityFrameworkCore;
using Tms.Api.Dtos;
using TmsApi.Data;
using TmsApi.Entities;

namespace Tms.Api.Services;

public class EnrollmentService(
    TmsDbContext context,
    ILogger<EnrollmentService> logger) : IEnrollmentService
{
    public async Task<List<EnrollmentResponseDto>> GetByCourseAsync(
    int courseId,
    CancellationToken ct)
{
    return await context.Enrollments
        .AsNoTracking()
        .Where(e => e.CourseId == courseId)
        .Select(e => new EnrollmentResponseDto(
            e.Id,
            e.StudentId,
            e.CourseId,
            e.EnrolledAt))
        .ToListAsync(ct);
}
    public Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct) =>
        context.Enrollments
            .AsNoTracking()
            .Where(e => e.Id == id && e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.StudentId,
                e.EnrolledAt))
            .FirstOrDefaultAsync(ct);

    public async Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        // Check if course exists
        var courseExists = await context.Courses
            .AnyAsync(c => c.Id == courseId, ct);

        if (!courseExists)
            throw new KeyNotFoundException("Course not found.");

        // Check if student exists
        var studentExists = await context.Students
            .AnyAsync(s => s.Id == request.StudentId, ct);

        if (!studentExists)
            throw new KeyNotFoundException("Student not found.");

        // Check duplicate enrollment
        var alreadyEnrolled = await context.Enrollments
            .AnyAsync(e =>
                e.CourseId == courseId &&
                e.StudentId == request.StudentId, ct);

        if (alreadyEnrolled)
            throw new InvalidOperationException("Student is already enrolled.");

        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow
        };

        context.Enrollments.Add(enrollment);

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Student {StudentId} enrolled in Course {CourseId}. EnrollmentId={EnrollmentId}",
            enrollment.StudentId,
            enrollment.CourseId,
            enrollment.Id);

        return await GetByIdAsync(courseId, enrollment.Id, ct)
            ?? throw new InvalidOperationException("Enrollment could not be retrieved.");
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task GetAllAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}