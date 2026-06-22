using System.Security.Cryptography.X509Certificates;
public interface IStudentService
{
    Task<StudentRecord?> CreateAsync(string studentId, string name, string email, string[]? courseCodes);
    Task<StudentRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<StudentRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}

public class StudentService : IStudentService
{
    private readonly Dictionary<string, StudentRecord> _studentStore = new();
    private readonly ILogger<StudentService> _logger;

    public StudentService(ILogger<StudentService> logger)
    {
        this._logger = logger;
        // --- seed some data ---
        var s1 = new StudentRecord("1", "s1", "Alice Smith", "alice.smith@example.com", ["c1", "c2"]);
        var s3 = new StudentRecord("2", "s2", "Bob Johnson", "bob.johnson@example.com", ["c3"]);
        var s2 = new StudentRecord("3", "s3", "Charlie Brown", "charlie.brown@example.com", ["c4"]);

        _studentStore[s1.Id] = s1;
        _studentStore[s2.Id] = s2;
        _studentStore[s3.Id] = s3;
    }
    public Task<StudentRecord?> CreateAsync(string studentId, string name, string email, string[]? courseCodes)
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var record = new StudentRecord(id, studentId, name, email, courseCodes ?? []);
        _studentStore[id] = record;
        _logger.LogInformation("Created student {StudentId} record {StudentId}", studentId, id);
        return Task.FromResult<StudentRecord?>(record);
        
    }
    public Task<StudentRecord?> GetByIdAsync(string id)
    {
        _studentStore.TryGetValue(id, out var record);
        if (record is null)
        {
            _logger.LogWarning("Student {StudentId} not found", id);
        }
        return Task.FromResult(record);
    }
    public Task<IReadOnlyList<StudentRecord>> GetAllAsync()
    {
        IReadOnlyList<StudentRecord> all = _studentStore.Values.ToList();
        return Task.FromResult(all);
    }
    public Task<bool> DeleteAsync(string id)
    {
        var removed = _studentStore.Remove(id);
        if (removed)
            _logger.LogInformation("Deleted student {StudentId}", id);
        else
            _logger.LogWarning("Delete failed student {StudentId} not found", id);
        return Task.FromResult(removed);
    }
}