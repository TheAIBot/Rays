namespace WorkProgress;

internal sealed class WorkReporting : IWorkReporting, IWorkReportOwner
{
    private readonly ConcurrentHashSet<IWorkReport> _incompleteWorkReports = new ConcurrentHashSet<IWorkReport>();
    private readonly ConcurrentHashSet<IWorkReport> _completedWorkReports = new ConcurrentHashSet<IWorkReport>();
    private readonly ConcurrentHashSet<IWorkReport> _failedWorkReports = new ConcurrentHashSet<IWorkReport>();

    public IKnownSizeWorkReport CreateKnownSizeWorkReport(int maxProgress)
    {
        var workReport = new KnownSizeWorkReport(this, maxProgress);
        _incompleteWorkReports.Add(workReport);

        return workReport;
    }

    public IEnumerable<IWorkReport> GetIncompleteWorkReports() => _incompleteWorkReports;
    public IEnumerable<IWorkReport> GetCompleteWorkReports() => _completedWorkReports;
    public IEnumerable<IWorkReport> GetFailedWorkReports() => _failedWorkReports;
    public IEnumerable<IWorkReport> GetAllWorkReports() => GetIncompleteWorkReports().Concat(GetCompleteWorkReports()).Concat(GetFailedWorkReports());

    public void ReportWorkCompleted(IWorkReport workReport)
    {
        _incompleteWorkReports.Remove(workReport);
        _completedWorkReports.Add(workReport);
    }

    public void ReportWorkFailed(IWorkReport workReport)
    {
        _incompleteWorkReports.Remove(workReport);
        _failedWorkReports.Add(workReport);
    }
}