namespace Si.EntityFramework.Extension.MultiDbContext.Configuration
{
    public class MutiDbOptions
    {
        public string MasterConnectionString { get; set; }
        public List<SlaveNode> SlaveNodes { get; set; } = new();
    }
    public class SlaveNode
    {
        public string ConnectionString { get; set; }
    }
}
