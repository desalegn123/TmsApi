public record CreateStudentRequest(
    string StudentId,
    string Name,
    string Email,
    string[]? CourseCodes
);