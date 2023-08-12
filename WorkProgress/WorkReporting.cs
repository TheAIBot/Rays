using WorkProgress.WorkReports;
using WorkProgress.WorkReports.KnownSize;
using WorkProgress.WorkReports.UnknownSize;

namespace WorkProgress;

internal sealed class WorkReporting : IWorkReporting, IWorkReportOwner
{
    private readonly ConcurrentHashSet<IWorkReport> _incompleteWorkReports = new();
    private readonly ConcurrentHashSet<IWorkReport> _completedWorkReports = new();
    private readonly ConcurrentHashSet<IWorkReport> _failedWorkReports = new();

    public IKnownSizeWorkReport CreateKnownSizeWorkReport(int maxProgress)
    {
        var workReport = new KnownSizeWorkReport(this, maxProgress);
        _incompleteWorkReports.Add(workReport);

        return workReport;
    }

    public IUnknownSizeWorkReport CreateUnknownWorkReport()
    {
        var workReport = new UnknownWorkReport(this);
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