namespace WorkProgress;

public interface IWorkReporting
{
    IKnownSizeWorkReport CreateKnownSizeWorkReport(int maxProgress);
    IEnumerable<IWorkReport> GetIncompleteWorkReports();
    IEnumerable<IWorkReport> GetCompleteWorkReports();
    IEnumerable<IWorkReport> GetFailedWorkReports();
    IEnumerable<IWorkReport> GetAllWorkReports();
}
