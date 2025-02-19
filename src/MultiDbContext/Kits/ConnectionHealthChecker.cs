using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Si.EntityFramework.Extension.MultiDbContext.Kits
{

    public class ConnectionHealthChecker : BackgroundService
    {
        private readonly DatabaseConfiguration _config;
        private readonly ILogger<ConnectionHealthChecker> _logger;

        public ConnectionHealthChecker(
            DatabaseConfiguration config,
            ILogger<ConnectionHealthChecker> logger)
        {
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(_config.HealthCheckInterval), stoppingToken);

                foreach (var node in _config.SlaveNodes)
                {
                    try
                    {
                        using var conn = new SqlConnection(node.ConnectionString);
                        await conn.OpenAsync(stoppingToken);
                        using var cmd = new SqlCommand("SELECT 1", conn);
                        await cmd.ExecuteScalarAsync(stoppingToken);
                        node.IsHealthy = true;
                    }
                    catch
                    {
                        node.IsHealthy = false;
                        _logger.LogWarning($"Slave node {node.ConnectionString} is unhealthy");
                    }
                }
            }
        }
    }
}
