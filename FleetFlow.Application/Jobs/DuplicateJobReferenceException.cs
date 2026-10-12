namespace FleetFlow.Application.Jobs;

public sealed class DuplicateJobReferenceException : Exception
{
    public DuplicateJobReferenceException(
        string reference,
        Exception? innerException = null)
        : base(
            $"A job with reference '{reference}' already exists for this tenant.",
            innerException)
    {
    }
}