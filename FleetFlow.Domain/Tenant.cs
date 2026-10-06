namespace FleetFlow.Domain;

public sealed class Tenant
{
    private readonly List<Job> _jobs = [];

    private Tenant()
    {
    }

    public Tenant(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Tenant name is required.",
                nameof(name));
        }

        Id = Guid.NewGuid();
        Name = name.Trim();
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public IReadOnlyCollection<Job> Jobs => _jobs.AsReadOnly();

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Tenant name is required.",
                nameof(name));
        }

        Name = name.Trim();
    }
}