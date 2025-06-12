using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Resources.API.Health;
using System.Data.Common;
using Xunit;

namespace Resources.API.Tests.Health
{
    public class DatabaseHealthCheckTests : IDisposable
    {
        private readonly string _dbPath;
        private SqliteConnection _connection;

        public DatabaseHealthCheckTests()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
            _connection = new SqliteConnection($"Data Source={_dbPath}");
            _connection.Open();
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }

            try
            {
                if (File.Exists(_dbPath))
                {
                    File.Delete(_dbPath);
                }
            }
            catch (IOException)
            {
                // Ignore file deletion errors
            }
        }

        [Fact]
        public async Task CheckHealthAsync_WhenDatabaseIsHealthy_ReturnsHealthy()
        {
            // Arrange
            DbConnection Factory() => _connection;
            var healthCheck = new DatabaseHealthCheck(Factory);

            // Act
            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            // Assert
            Assert.Equal(HealthStatus.Healthy, result.Status);
        }

        [Fact]
        public async Task CheckHealthAsync_WhenDatabaseIsUnhealthy_ReturnsUnhealthy()
        {
            // Arrange
            _connection.Close();
            _connection.Dispose();
            
            // Use a non-existent path that requires write access
            var nonExistentPath = Path.Combine(Path.GetTempPath(), "NonExistentFolder", "db.sqlite");
            _connection = new SqliteConnection($"Data Source={nonExistentPath};Mode=ReadWriteCreate");
            DbConnection Factory() => _connection;
            var healthCheck = new DatabaseHealthCheck(Factory);

            // Act
            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            // Assert
            Assert.Equal(HealthStatus.Unhealthy, result.Status);
            Assert.Contains("Database is unhealthy", result.Description);
        }
    }
} 