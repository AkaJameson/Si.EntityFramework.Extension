namespace Si.EntityFramework.Extension.Entitys
{
    public class QueryMetrics
    {
        public int ExecutionCount { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public TimeSpan AverageDuration => TimeSpan.FromTicks(TotalDuration.Ticks / ExecutionCount);
        public DateTime LastExecuted { get; set; }
    }
}
