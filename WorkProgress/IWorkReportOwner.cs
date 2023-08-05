namespace WorkProgress;

internal interface IWorkReportOwner
{
    void ReportWorkCompleted(IWorkReport workReport);
    void ReportWorkFailed(IWorkReport workReport);
}
