using Si.EntityFramework.Extension.Database;
using Si.EntityFramework.Extension.MultiDbContext.Configuration;

namespace Si.EntityFramework.Extension.MultiDbContext
{
    public partial class DbContextRouter<TContext> where TContext : ApplicationDbContext
    {
        private MutiDbOptions _options;
        private readonly List<string> _slaveConnections = new List<string>();
        public bool IsReadOperation { get; set; } = false;
        public string PendingConnection { get; set; } = string.Empty;
        private int _currentIndex = 0;
        public DbContextRouter(MutiDbOptions options)
        {
            _options = options;
            if (_options.MasterConnectionString == null||_options.SlaveNodes.Count == 0)
            {
                throw new ArgumentException("MasterConnectionString or SlaveNodes is null or empty.");
            }
            _slaveConnections = _options.SlaveNodes.Select(x => x.ConnectionString).ToList();
        }
        public string GetReadConnection() => GetNextSlaveConnection();
        public string GetWriteConnection() => _options.MasterConnectionString;
        private string GetNextSlaveConnection()
        {
            lock (this)
            {
                var connection = _slaveConnections[_currentIndex];
                _currentIndex = (_currentIndex + 1) % _slaveConnections.Count;
                return connection;
            }
        }
    }
}
