using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/students")]

public class StudetnsController(IStudentService studentService) : ControllerBase
{
    // GET/api/students returns all student records
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var students = await studentService.GetAllAsync();
        return Ok(students);
    }

    // GET/api/students/{id} returns one or 404
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var record = await studentService.GetByIdAsync(id);
        return record is not null ? Ok(record) : NotFound();
    }

    // POST /api/students creates and returns 201 with Location header
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
    {
        var record = await studentService.CreateAsync(request.StudentId, request.Name, request.Email, request.CourseCodes);
        return CreatedAtAction(nameof(GetById), new { id = record?.Id }, record);
    }

    // DELETE /api/students/{id} returns 204 or 404
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await studentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}