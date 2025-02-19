namespace Si.EntityFramework.Extension.MultiDbContext.Configuration
{
    public class MutiDbConfiguration
    {
        public string MasterConnectionString { get; set; }
        public List<SlaveNode> SlaveNodes { get; set; } = new();
        public int HealthCheckInterval { get; set; } = 30;
    }
    public class SlaveNode
    {
        public string ConnectionString { get; set; }
        public bool IsHealthy { get; set; } = true;
    }
}
