using Si.EntityFramework.Extension.Database;
using Si.EntityFramework.Extension.DataBase.Configuration;

namespace Si.EntityFramework.Extension.DataBase
{
    public partial class DbContextRouter<TContext> where TContext : ApplicationDbContext
    {
        public DbContextRouter(MutiDbOptions mutiDbOptions)
        {
            if (mutiDbOptions.MasterConnectionString == null || mutiDbOptions.SlaveNodes.Count == 0)
            {
                throw new ArgumentException("MasterConnectionString or SlaveNodes is null or empty.");
            }
            MasterConnectionString = mutiDbOptions.MasterConnectionString;
            _slaveConnections = mutiDbOptions.SlaveNodes.Select(x => x.ConnectionString).ToList();
        }
        internal static string MasterConnectionString { get; set; }

        internal static List<string> _slaveConnections = new List<string>();

        public bool IsReadOperation { get; set; } = false;
        internal bool ForceDbMaster = false;
        public string PendingConnection { get; set; } = string.Empty;

        private static int _currentIndex = 0;

        public string GetReadConnection() => GetNextSlaveConnection();
        public string GetWriteConnection() => MasterConnectionString;
        private string GetNextSlaveConnection()
        {
            lock (this)
            {
                var connection = _slaveConnections[_currentIndex];
                _currentIndex = (_currentIndex + 1) % _slaveConnections.Count;
                return connection;
            }
        }
        public void ForceMaster() => ForceDbMaster = true;
    }
}
