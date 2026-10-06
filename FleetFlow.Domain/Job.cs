namespace FleetFlow.Domain;

public sealed class Job
{
    private Job()
    {
    }

    public Job(
        Guid tenantId,
        string reference,
        string description)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException(
                "Tenant ID is required.",
                nameof(tenantId));
        }

        if (string.IsNullOrWhiteSpace(reference))
        {
            throw new ArgumentException(
                "Job reference is required.",
                nameof(reference));
        }

        Id = Guid.NewGuid();
        TenantId = tenantId;
        Reference = reference.Trim();
        Description = description?.Trim() ?? string.Empty;
        Status = JobStatus.Pending;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Reference { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public JobStatus Status { get; private set; }

    public void Start()
    {
        if (Status != JobStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending jobs can be started.");
        }

        Status = JobStatus.InProgress;
    }

    public void Complete()
    {
        if (Status != JobStatus.InProgress)
        {
            throw new InvalidOperationException(
                "Only jobs in progress can be completed.");
        }

        Status = JobStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == JobStatus.Completed)
        {
            throw new InvalidOperationException(
                "A completed job cannot be cancelled.");
        }

        Status = JobStatus.Cancelled;
    }
}
