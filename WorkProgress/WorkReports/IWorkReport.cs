namespace WorkProgress.WorkReports;

public interface IWorkReport
{
    int Progress { get; }
    TimeSpan WorkTime { get; }
    void IncrementProgress();
}