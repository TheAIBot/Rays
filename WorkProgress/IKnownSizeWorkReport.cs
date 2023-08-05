namespace WorkProgress;

public interface IKnownSizeWorkReport : IWorkReport, IDisposable
{
    int MaxProgress { get; }
    TimeSpan WorkTime { get; }
    void ReportProgress(int progress);
}
