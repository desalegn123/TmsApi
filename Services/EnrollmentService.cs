// --- The contract ---
using System.Security.Cryptography.X509Certificates;

public interface IEnrollmentService
{
    Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode);
    Task<EnrollmentRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}
// --- The in-memory implementation ---

public class EnrollmentService : IEnrollmentService
{
    private readonly Dictionary<string, EnrollmentRecord> _enrollmentStore = new();
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(ILogger<EnrollmentService> logger)
    {
        _logger = logger;

        // --- seed some data ---
        var e1 = new EnrollmentRecord("1", "1", "c1", DateTime.UtcNow.AddDays(-10));
        var e2 = new EnrollmentRecord("2", "1", "c2", DateTime.UtcNow.AddDays(-9));
        var e3 = new EnrollmentRecord("3", "2", "c3", DateTime.UtcNow.AddDays(-8));

        _enrollmentStore[e1.Id] = e1;
        _enrollmentStore[e2.Id] = e2;
        _enrollmentStore[e3.Id] = e3;
    }
    public Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode)
    {
        var existing = _enrollmentStore.Values
            .FirstOrDefault(e => e.StudentId == studentId && e.CourseCode == courseCode);

        if (existing is not null)
        {
            _logger.LogWarning("Duplicate enrollment attemt {StudentId} already in {CourseCode} (record {EnrolmentId})",
            studentId, courseCode, existing.Id);
            return Task.FromResult(existing);
        }

        var id = Guid.NewGuid().ToString("N")[..8];
        var record = new EnrollmentRecord(id, studentId, courseCode, DateTime.UtcNow);
        _enrollmentStore[id] = record;
        _logger.LogInformation("Enrolled {StudentId} in {CourseCode} record {EnrollmentId}",
                studentId, courseCode, id);
        return Task.FromResult(record);
    }
    public Task<EnrollmentRecord?> GetByIdAsync(string id)
    {
        _enrollmentStore.TryGetValue(id, out var record);
        if (record is null)
        {
            _logger.LogWarning("Enrolment {EnrollmentId} not found", id);
        }
        return Task.FromResult(record);
    }
    public Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
    {
        IReadOnlyList<EnrollmentRecord> all = _enrollmentStore.Values.ToList();
        return Task.FromResult(all);
    }
    public Task<bool> DeleteAsync(string id)
    {
        var removed = _enrollmentStore.Remove(id);
        if (removed)
            _logger.LogInformation("Deleted enrollment {EnrollmentId}", id);
        else
            _logger.LogWarning("Delete failed enrollment {EnrollmentId} not found", id);
        return Task.FromResult(removed);
    }
}

public class TmsDatabaseException(string message) : Exception(message);
