using WorkProgress.WorkReports;
using WorkProgress.WorkReports.KnownSize;
using WorkProgress.WorkReports.UnknownSize;

namespace WorkProgress;

public interface IWorkReporting
{
    IKnownSizeWorkReport CreateKnownSizeWorkReport(int maxProgress);
    IUnknownSizeWorkReport CreateUnknownWorkReport();
    IEnumerable<IWorkReport> GetIncompleteWorkReports();
    IEnumerable<IWorkReport> GetCompleteWorkReports();
    IEnumerable<IWorkReport> GetFailedWorkReports();
    IEnumerable<IWorkReport> GetAllWorkReports();
}
