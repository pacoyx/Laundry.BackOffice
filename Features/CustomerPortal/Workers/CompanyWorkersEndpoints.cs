public static class CompanyWorkersEndpoints
{
    internal static CompanyWorkerResponse ToResponse(CompanyWorker worker) => new(
        worker.Id, worker.Code, worker.FullName, worker.Area, worker.IsActive
    );
}
