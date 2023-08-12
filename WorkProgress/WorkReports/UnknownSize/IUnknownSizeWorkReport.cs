namespace WorkProgress.WorkReports.UnknownSize;

public interface IUnknownSizeWorkReport : IWorkReport, IDisposable
{
    void Complete();
    void Fail();
}