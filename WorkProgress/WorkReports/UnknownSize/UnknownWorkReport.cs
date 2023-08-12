namespace WorkProgress.WorkReports.UnknownSize;

internal sealed class UnknownWorkReport : IUnknownSizeWorkReport
{
    private readonly IWorkReportOwner _workReportOwner;
    private int _progress = 0;
    private bool _stillWorking = true;
    public int Progress => _progress;

    public UnknownWorkReport(IWorkReportOwner workReportOwner)
    {
        _workReportOwner = workReportOwner;
    }

    public void IncrementProgress()
    {
        _progress++;
    }

    public void Complete()
    {
        if (!_stillWorking)
        {
            throw new InvalidOperationException("Can not complete an already completed work report.");
        }

        _stillWorking = false;
        _workReportOwner.ReportWorkCompleted(this);
    }

    public void Fail()
    {
        if (!_stillWorking)
        {
            throw new InvalidOperationException("Can not complete an already completed work report.");
        }

        _stillWorking = false;
        _workReportOwner.ReportWorkFailed(this);
    }

    public void Dispose()
    {
        if (_stillWorking)
        {
            Fail();
            return;
        }
    }
}
