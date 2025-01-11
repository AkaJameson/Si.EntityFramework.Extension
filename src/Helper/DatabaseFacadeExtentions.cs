using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Data.Common;

namespace Si.Framework.EntityFramework.Extension
{
    public static class DatabaseFacadeExtentions
    {
        public static ICollection<T> FromSqlCollection<T>(
            this DatabaseFacade databaseFacade,
            string commandText,
            params object[] commandParameters)
            where T : new()
        {
            if (databaseFacade == null)
            {
                throw new ArgumentNullException(nameof(databaseFacade));
            }

            return ExecuteDataReaderCollection<T>(
                databaseFacade,
                CommandType.Text,
                commandText,
                commandParameters);
        }

        public static async Task<ICollection<T>> FromSqlCollectionAsync<T>(
            this DatabaseFacade databaseFacade,
            string commandText,
            params object[] commandParameters)
            where T : new()
        {
            if (databaseFacade == null)
            {
                throw new ArgumentNullException(nameof(databaseFacade));
            }

            return await ExecuteDataReaderCollectionAsync<T>(
                 databaseFacade,
                 CommandType.Text,
                 commandText,
                 commandParameters);
        }

        static ICollection<T> ExecuteDataReaderCollection<T>(
            DatabaseFacade databaseFacade,
            CommandType commandType,
            string commandText,
            params object[] commandParameters)
            where T : new()
        {
            var dbConnection = databaseFacade
                .GetService<IRelationalConnection>();

            IDataReader dataReader;

            try
            {
                var dbCommand = CreateCommand(
                    dbConnection,
                    commandType,
                    commandText,
                    commandParameters);

                dbConnection.Open();
                dataReader = dbCommand.ExecuteReader();
            }
            catch
            {
                dbConnection.Close();
                throw;
            }

            try
            {
                using (dataReader)
                {
                    return dataReader.ToObjectCollection<T>();
                }
            }
            finally
            {
                dbConnection.Close();
            }
        }

        static async Task<ICollection<T>> ExecuteDataReaderCollectionAsync<T>(
            DatabaseFacade databaseFacade,
            CommandType commandType,
            string commandText,
            params object[] commandParameters) where T : new()
        {
            CancellationToken cancellationToken = CancellationToken.None;

            var dbConnection = databaseFacade
                .GetService<IRelationalConnection>();

            IDataReader dataReader;

            try
            {
                var dbCommand = CreateCommand(
                    dbConnection,
                    commandType,
                    commandText,
                    commandParameters);

                await dbConnection.OpenAsync(cancellationToken);
                dataReader = await dbCommand.ExecuteReaderAsync(cancellationToken);
            }
            catch
            {
                dbConnection.Close();
                throw;
            }

            try
            {
                using (dataReader)
                {
                    return dataReader.ToObjectCollection<T>();
                }
            }
            finally
            {
                dbConnection.Close();
            }
        }

        static DbCommand CreateCommand(
            IRelationalConnection connection,
            CommandType commandType,
            string commandText,
            params object[] commandParameters)
        {
            var command = connection.DbConnection.CreateCommand();

            command.CommandType = commandType;
            command.CommandText = commandText;

            if (connection.CurrentTransaction != null)
            {
                command.Transaction = connection
                    .CurrentTransaction.GetDbTransaction();
            }

            if (connection.CommandTimeout != null)
            {
                command.CommandTimeout = (int)connection.CommandTimeout;
            }

            if (commandParameters != null && commandParameters.Length > 0)
            {
                foreach (var parameter in commandParameters)
                {
                    command.Parameters.Add(parameter);
                }
            }

            return command;
        }
        public static DataTable SqlQuery(this DatabaseFacade databaseFacade,
           CommandType commandType,
           string commandText,
           params object[] commandParameters)
        {
            if (databaseFacade == null)
            {
                throw new ArgumentNullException(nameof(databaseFacade));
            }
            try
            {
                DataTable dt = new DataTable();
                using var dbConnection = databaseFacade.GetService<IRelationalConnection>();
                dbConnection.Open();
                using var dbCommand = CreateCommand(dbConnection, commandType, commandText, commandParameters);
                using IDataReader dataReader = dbCommand.ExecuteReader();
                dt.Load(dataReader);
                return dt;
            }
            catch
            {
                throw;
            }
        }


        public static async Task<DataTable> SqlQueryAsync(this DatabaseFacade databaseFacade,
            CommandType commandType,
            string commandText,
            params object[] commandParameters)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            if (databaseFacade == null)
            {
                throw new ArgumentNullException(nameof(databaseFacade));
            }
            try
            {
                DataTable dt = new DataTable();
                using var dbConnection = databaseFacade.GetService<IRelationalConnection>();
                await dbConnection.OpenAsync(cancellationToken);
                using var dbCommand = CreateCommand(dbConnection, commandType, commandText, commandParameters);
                using IDataReader dataReader = await dbCommand.ExecuteReaderAsync(cancellationToken);
                dt.Load(dataReader);
                return dt;
            }
            catch
            {
                throw;
            }
        }
    }
}