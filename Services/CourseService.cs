using System.Security.Cryptography.X509Certificates;
public interface ICourseService
{
    Task<CourseRecord?> CreateAsync(string courseCode, string name, string description);
    Task<CourseRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<CourseRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}

public class CourseService : ICourseService
{
    private readonly Dictionary<string, CourseRecord> _courseStore = new();
    private readonly ILogger<CourseService> _logger;

    public CourseService(ILogger<CourseService> logger)
    {
        this._logger = logger;
        // --- seed some data ---
        var c1 = new CourseRecord("1", "c1", "Introduction to Programming", "Learn the basics of programming using C#.");
        var c2 = new CourseRecord("2", "c2", "Data Structures", "Explore common data structures and their applications.");
        var c3 = new CourseRecord("3", "c3", "Web Development", "Build modern web applications using ASP.NET Core.");
        var c4 = new CourseRecord("4", "c4", "Database Systems", "Understand relational databases and SQL.");

        _courseStore[c1.Id] = c1;
        _courseStore[c2.Id] = c2;
        _courseStore[c3.Id] = c3;
        _courseStore[c4.Id] = c4;
    }
    public Task<CourseRecord?> CreateAsync(string courseCode, string name, string description)
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var record = new CourseRecord(id, courseCode, name, description);
        _courseStore[id] = record;
        _logger.LogInformation("Created course {CourseCode} record {CourseId}", courseCode, id);
        return Task.FromResult<CourseRecord?>(record);
    }
    public Task<CourseRecord?> GetByIdAsync(string id)
    {
        _courseStore.TryGetValue(id, out var record);
        if (record is null)
        {
            _logger.LogWarning("Course {CourseId} not found", id);
        }
        return Task.FromResult(record);
    }
    public Task<IReadOnlyList<CourseRecord>> GetAllAsync()
    {
        IReadOnlyList<CourseRecord> all = _courseStore.Values.ToList();
        return Task.FromResult(all);
    }
    public Task<bool> DeleteAsync(string id)
    {
        var removed = _courseStore.Remove(id);
        if (removed)
            _logger.LogInformation("Deleted course {CourseId}", id);
        else
            _logger.LogWarning("Delete failed course {CourseId} not found", id);
        return Task.FromResult(removed);
    }
}