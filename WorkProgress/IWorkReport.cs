namespace WorkProgress;

public interface IWorkReport
{
    int Progress { get; }
    void IncrementProgress();
}
