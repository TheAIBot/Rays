namespace WorkProgress.WorkReports.KnownSize;

internal sealed class KnownSizeWorkReport : IKnownSizeWorkReport
{
    private readonly IWorkReportOwner _workReportOwner;
    private readonly DateTime _workStartTime;
    private DateTime _workEndTime;

    public int Progress { get; private set; } = 0;
    public int MaxProgress { get; }
    public TimeSpan WorkTime => _workEndTime - _workStartTime;

    public KnownSizeWorkReport(IWorkReportOwner workReportOwner, int maxProgress)
    {
        _workReportOwner = workReportOwner;
        MaxProgress = maxProgress;
        _workStartTime = DateTime.UtcNow;
        _workEndTime = default;
    }

    public void ReportProgress(int progress)
    {
        if (progress < 0 || progress > MaxProgress)
        {
            throw new ArgumentOutOfRangeException(nameof(progress), progress, $"Progress can not be less than 0 or larger than {nameof(MaxProgress)}: {MaxProgress}.");
        }

        if (Progress == MaxProgress)
        {
            throw new InvalidOperationException($"Not allowed to change progress once progress reaches max progress.");
        }

        Progress = progress;
        ReportCompletedWhenCompleted();
    }

    public void IncrementProgress()
    {
        if (Progress == MaxProgress)
        {
            throw new InvalidOperationException($"Not allowed to increment progress above {nameof(MaxProgress)}: {MaxProgress}.");
        }

        Progress++;
        if (Progress % 100 == 0)
        {
            Console.WriteLine($"Progress: {Progress}/{MaxProgress}");
        }
        ReportCompletedWhenCompleted();
    }

    private void ReportCompletedWhenCompleted()
    {
        if (Progress != MaxProgress)
        {
            return;
        }

        _workEndTime = DateTime.UtcNow;
        _workReportOwner.ReportWorkCompleted(this);
    }

    public void Dispose()
    {
        if (Progress == MaxProgress)
        {
            return;
        }

        _workEndTime = DateTime.UtcNow;
        _workReportOwner.ReportWorkFailed(this);
    }
}