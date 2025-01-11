namespace Si.Framework.EntityFramework.Entity
{
    public class QueryMetrics
    {
        public int ExecutionCount { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public TimeSpan AverageDuration => TimeSpan.FromTicks(TotalDuration.Ticks / ExecutionCount);
        public DateTime LastExecuted { get; set; }
    }
}
