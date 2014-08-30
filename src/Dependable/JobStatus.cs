namespace Dependable
{
    public enum JobStatus
    {
        Created,
        Ready,
        Running,
        WaitingForChildren,
        Failed,
        ReadyToComplete,
        Completed,
        ReadyToPoison,
        Poisoned
    }
}