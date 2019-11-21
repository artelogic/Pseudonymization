using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pseudonymization.Core.Attributes;
using Pseudonymization.Core.ConnectionAnalysis;
using Pseudonymization.Core.Logger;
using Pseudonymization.Core.Pseudonymizers;
using Pseudonymization.Core.SchemaAnalysis;

namespace Pseudonymization.Core.Providers
{
    [Provider(
        Name = nameof(SqlServerPseudonymizationProvider),
        Description = "Pseudonymizer for SQL Server databases",
        ConnectionAnalyzerType = typeof(SqlServerConnectionAnalyzer))]
    public class SqlServerPseudonymizationProvider : PseudonymizationProviderBase
    {
        private const int BulkSize = 1000;
        private long _bulkSizeExpected;
        private long _currentBulkIndexProcessed;

        public SqlServerPseudonymizationProvider(IPseudonymizer pseudonymizer, string connectionString, ILogger logger)
            : base(pseudonymizer, connectionString, logger)
        {
        }

        public override event ProgressUpdatedEventHandler OnProgressUpdated;
        public override event PseudonymizationFailedEventHandler OnPseudonymizationFailed;
        public override event PseudonymizationSuccessfulEventHandler OnPseudonymizationSecceeded;

        public override Task<IEnumerable<PseudonymizationSchemaRepresentation>> GetPseudonymizationColumnsFromSchemaAsync(CancellationToken cancellationToken)
        {
            return Task.Run((Func<IEnumerable<PseudonymizationSchemaRepresentation>>) GetPseudonymizationColumnsFromSchema, cancellationToken);
        }

        public IEnumerable<PseudonymizationSchemaRepresentation> GetPseudonymizationColumnsFromSchema()
        {
            using (var analyzer = new SqlServerSchemaAnalyzer(new SqlConnection(ConnectionString), Patterns))
            {
                var schemaList = analyzer.GetAvailableSchemas();

                IEnumerable<PseudonymizationSchemaRepresentation> data = schemaList
                    .AsParallel()
                    .Select(schema => new PseudonymizationSchemaRepresentation(schema, analyzer.GetTablesAndColumns(schema)))
                    .ToList();

                return data;
            }
        }

        public override Task PseudonymizeAsync(IEnumerable<PseudonymizationSchemaRepresentation> schemaList)
        {
            return Task.Run(() =>
            {
                try
                {
                    _bulkSizeExpected = GetTotalBulkSize(schemaList);

                    foreach (var schema in schemaList)
                    {
                        PseudonymizeSchema(schema);
                    }

                    OnPseudonymizationSecceeded?.Invoke(null, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    OnPseudonymizationFailed?.Invoke(null, EventArgs.Empty);
#if DEBUG
                    throw;
#else
                    Logger.Log(ex);
#endif
                    //throw;
                }
            });
        }

        private void PseudonymizeSchema(PseudonymizationSchemaRepresentation schema)
        {
            foreach (var table in schema.Tables)
            {
                PseudonymizeTable(schema.SchemaName, table);
            }
        }

        private long GetTotalBulkSize(IEnumerable<PseudonymizationSchemaRepresentation> schemaRepresentations)
        {
            long size = 0;

            foreach(var schema in schemaRepresentations)
            {
                foreach (var tbl in schema.Tables)
                {
                    checked
                    {
                        size += (tbl.BulkBlocks = GetTableSize(schema.SchemaName, tbl.Name) / BulkSize + 1);
                    }
                }
            }

            return size;
        }

        public void PseudonymizeTable(string schema, Table table)
        {
            string sql = $"SELECT * FROM [{schema}].[{table.Name}] ORDER BY {table.Columns.First().ColumnName} ASC OFFSET {{0}} ROWS FETCH NEXT {BulkSize} ROWS ONLY";

            long size = table.BulkBlocks;

            for (long i = 0; i < size; i++)
            {
                using (var dataTable = new DataTable())
                using (var sqlConn = new SqlConnection(ConnectionString))
                using (var cmd = new SqlDataAdapter(string.Format(sql, i * BulkSize), sqlConn))
                using (var sqlCommand = new SqlCommandBuilder(cmd))
                {
                    try
                    {
                        cmd.Fill(dataTable);

                        for (int j = 0; j < dataTable.Rows.Count; j++)
                        {
                            foreach (var column in table.Columns)
                            {
                                var pseudonymized = Pseudonymizer.Pseudonymize(dataTable.Rows[j][column.ColumnName].ToString());
                                string value = column.MaxLength.HasValue && pseudonymized.Length > column.MaxLength.Value
                                    ? new string(pseudonymized.Take(column.MaxLength.Value).ToArray())
                                    : pseudonymized;
                                dataTable.Rows[j][column.ColumnName] = value;
                            }
                        }

                        cmd.UpdateCommand = sqlCommand.GetUpdateCommand();
                        cmd.UpdateBatchSize = BulkSize;
                        cmd.Update(dataTable);

                        int progressPercentage = (int)(((double)(++_currentBulkIndexProcessed) / _bulkSizeExpected) * 100);

                        OnProgressUpdated?.Invoke(
                            null, 
                            new ProgressEventArgs()
                            {
                                Percetange = progressPercentage
                            });
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }
                }
            }
        }

        private long GetTableSize(string schema, string table)
        {
            using (var sqlConn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand($"SELECT CONVERT(BIGINT, COUNT(*)) AS SIZE FROM [{schema}].[{table}]", sqlConn))
            {
                try
                {
                    sqlConn.Open();
                    return (long)cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    return 0;
                }
            }
        }

        protected override bool IsAccessible()
        {
            bool flag = false;

            try
            {
                using (var sqlConn = new SqlConnection(ConnectionString))
                {
                    sqlConn.Open();
                    flag = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return flag;
        }
    }
}
