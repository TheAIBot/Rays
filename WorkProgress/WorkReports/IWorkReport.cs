namespace WorkProgress.WorkReports;

public interface IWorkReport
{
    int Progress { get; }
    void IncrementProgress();
}