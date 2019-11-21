using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using Pseudonymization.Core.Exceptions;

namespace Pseudonymization.Core.ConnectionAnalysis
{
    public class SqlServerConnectionAnalyzer : IConnectionStringAnalyzer
    {
        const string ConfigurationName = "SqlServer";
        public SqlServerConnectionAnalyzer()
        {
        }

        public bool IsMatch(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return false;
            }

            var matches = Regex.Matches(connectionString, "(?:^|;)(\"(?:[^\"]+|\"\")*\"|[^;]*)(\'(?:[^\']+|\'\')*\'|[^;]*)");

            var connectionBlocks = matches.Cast<Match>().Select(m => m.Value.TrimStart(';')).ToArray();
            //var connectionBlocks = connectionString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim()).ToArray();

            if (!connectionBlocks.Any())
            {
                return false;
            }

            var connectionKeyValues = connectionBlocks.Select(SplitConnectionBlock).ToList();
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
            connectionKeyValues.ForEach(kw => sqlConnectionStringBuilder[kw.Key] = kw.Value);
            bool isMatch = false;

            try
            {
                using (var sqlConn = new SqlConnection(sqlConnectionStringBuilder.ConnectionString))
                {
                    sqlConn.Open();
                    isMatch = true;
                }
            }
            catch (SqlException ex)
            {
                isMatch = false;
            }
            catch(Exception ex)
            {
                isMatch = false;
                // log here also
            }

            return isMatch;
        }

        public KeyValuePair<string, string> SplitConnectionBlock(string connectionBlock)
        {
            int indexOfFirstAssignment = connectionBlock.IndexOf('=');

            if (indexOfFirstAssignment < 0)
            {
                throw new InvalidConnectionBlockException($"Block: {connectionBlock}");
            }

            var key = new string(connectionBlock.Take(indexOfFirstAssignment++).ToArray());
            var value = new string(connectionBlock.Skip(indexOfFirstAssignment).ToArray());

            return new KeyValuePair<string, string>(key, value);
        }
    }
}
