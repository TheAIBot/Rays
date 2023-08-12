namespace WorkProgress.WorkReports.KnownSize;

public interface IKnownSizeWorkReport : IWorkReport, IDisposable
{
    int MaxProgress { get; }
    void ReportProgress(int progress);
}
