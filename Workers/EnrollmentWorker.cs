// public class EnrollmentWorker
// {
//     IEnrollmentService  enrollmentService1;
//     public EnrollmentWorker(IEnrollmentService enrollmentService)
//     {
//         enrollmentService1 = enrollmentService;
//     }
// }

// Inside EnrollmentWorker.cs...
public class EnrollmentWorker(IServiceScopeFactory scopeFactory)
{
    public void ProcessBatch()
    {
        // TODO2:Create a short-lived scope using the injected factory.
        var scope = scopeFactory.CreateScope();
        // TODO3:Resolve the scoped service from the new scope's provider.
        var svc = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
        // TODO4:Usetheservice, then let the 'using' block dispose the scope
        // and its scoped services automatically.
    }
}
