using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Pseudonymization.Core.Exceptions;

namespace Pseudonymization.Core.SchemaAnalysis
{
    public sealed class SqlServerSchemaAnalyzer : ISchemaAnalyzer, IDisposable
    {
        private const string TableNameKey = "TABLE_NAME";
        private const string ColumnNameKey = "COLUMN_NAME";
        private const string ColumnSizeKey = "COLUMN_LENGTH";
        private const string SchemaNameKey = "SCHEMA_NAME";
        private SqlConnection _dbConnection;
        private readonly string[] _triggerKeywords;
        private bool disposedValue = false;

        private SqlServerSchemaAnalyzer()
        {
        }

        public SqlServerSchemaAnalyzer(SqlConnection dbConnection, string[] triggerKeywords)
        {
            _dbConnection = dbConnection;
            _triggerKeywords = triggerKeywords;
        }

        public HashSet<string> GetAvailableSchemas()
        {
            string query = $"SELECT DISTINCT TABLE_SCHEMA AS {SchemaNameKey} FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";

            try
            {
                using (var dataTable = new DataTable())
                using (var adapter = new SqlDataAdapter(query, _dbConnection) { AcceptChangesDuringFill = false })
                {
                    adapter.Fill(dataTable);
                    return ExtractSchemasFromQueryResult(dataTable);
                }
            }
            catch (Exception ex)
            {
                throw new SchemaAnalysisFailedException("Available schemas get error");
            }
        }

        public HashSet<Table> GetTablesAndColumns(string schemaName)
        {
            string where = string.Empty;
            if (_triggerKeywords.Any())
            {
                where = _triggerKeywords.Select(kw => $"{ColumnNameKey} LIKE '{kw}'").Aggregate((f, l) => $"{f} OR {l}");
                where = string.IsNullOrEmpty(where) ? string.Empty : $" AND ({where})";
            }

            string cQuery = string.Empty
                + $"SELECT DISTINCT TABLE_NAME AS {TableNameKey} FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA = '{schemaName}'"
                + Environment.NewLine
                + $"SELECT DISTINCT INFORMATION_SCHEMA.COLUMNS.TABLE_NAME AS {TableNameKey}, INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME AS {ColumnNameKey}, INFORMATION_SCHEMA.COLUMNS.CHARACTER_MAXIMUM_LENGTH AS {ColumnSizeKey} "
                + "FROM INFORMATION_SCHEMA.TABLES "
                + "JOIN INFORMATION_SCHEMA.COLUMNS ON INFORMATION_SCHEMA.COLUMNS.TABLE_NAME = INFORMATION_SCHEMA.TABLES.TABLE_NAME "
                + $@"WHERE TABLE_TYPE='BASE TABLE' AND INFORMATION_SCHEMA.TABLES.TABLE_SCHEMA = '{schemaName}' {where} "
                + "GROUP BY INFORMATION_SCHEMA.COLUMNS.TABLE_NAME, INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME, INFORMATION_SCHEMA.COLUMNS.CHARACTER_MAXIMUM_LENGTH";

            try
            {
                using (var ds = new DataSet())
                using (var adapter = new SqlDataAdapter(cQuery, _dbConnection) { AcceptChangesDuringFill = false })
                {
                    adapter.Fill(ds);

                    var tables = ExtractTablesFromQueryResult(ds.Tables[0]);
                    return ExtractColumnsForTables(tables, ds.Tables[1]);
                }
            }
            catch (Exception ex)
            {
                throw new SchemaAnalysisFailedException(schemaName, ex);
            }
        }

        private HashSet<string> ExtractSchemasFromQueryResult(DataTable table)
        {
            return GetHashSetFromTableByKey(table, SchemaNameKey);
        }

        private HashSet<string> ExtractTablesFromQueryResult(DataTable table)
        {
            return GetHashSetFromTableByKey(table, TableNameKey);
        }

        private HashSet<Table> ExtractColumnsForTables(HashSet<string> tables, DataTable columns)
        {
            var data = new HashSet<Table>();

            foreach (var item in tables)
            {
                var columnNames = ExtractColumnsForTable(item, columns);

                if (columnNames.Any())
                {
                    data.Add(new Table() { Name = item, Columns = columnNames });
                }
            }

            return data;
        }

        private HashSet<ColumnMetadata> ExtractColumnsForTable(string tableName, DataTable dbResult)
        {
            var selSet = dbResult
                .Select($"{TableNameKey} = '{tableName}'")
                .Select(r => new ColumnMetadata(r[ColumnNameKey].ToString()) { MaxLength = (int?)r[ColumnSizeKey] })
                .ToList();

            return new HashSet<ColumnMetadata>(selSet);
        }

        private HashSet<string> GetHashSetFromTableByKey(DataTable table, string key)
        {
            var result = table
                .Rows
                .Cast<DataRow>()
                .Select(r => r[key].ToString());

            return new HashSet<string>(result);
        }

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _dbConnection?.Dispose();
                }

                _dbConnection = null;

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
