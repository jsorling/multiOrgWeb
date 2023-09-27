using Sorling.SqlExec.runner;
using System.Data;
using System.Data.SqlClient;

namespace Sorling.sqlSessionRunner;

public class SqlSessionRunner : SqlExecRunner, ISqlSessionRunner
{
   private readonly IDictionary<string, string> _sessionkeys;

   public SqlSessionRunner(string sqlConnectionString, IDictionary<string, string> sessionKeys) : base(sqlConnectionString) {
      if (sessionKeys is null || !sessionKeys.Any())
         throw new ArgumentOutOfRangeException(nameof(sessionKeys), "Session keys can not be null or empty");

      if (sessionKeys.Any(c => string.IsNullOrWhiteSpace(c.Key) || c.Key.Any(h => !char.IsLetterOrDigit(h)))) {
         string nonvalidkey = sessionKeys.FirstOrDefault(c => string.IsNullOrWhiteSpace(c.Key)
         || c.Key.Any(h => !char.IsLetterOrDigit(h))).Key ?? "<null>";
         throw new ArgumentOutOfRangeException(nameof(sessionKeys), $"Key '{nonvalidkey}' is not valid as a key name");
      }

      _sessionkeys = sessionKeys;
   }

   private SqlCommand BuildCommand(SqlConnection sqlConnection) {
      SqlCommand cmd = sqlConnection.CreateCommand();
      cmd.CommandType = CommandType.Text;

      foreach (KeyValuePair<string, string> kvp in _sessionkeys) {
         string keypar = "@" + kvp.Key;
         string valuepar = keypar + "value";
         cmd.CommandText += $"exec sys.sp_set_session_context {keypar}, {valuepar}, 1;";

         _ = cmd.Parameters.AddWithValue(keypar, kvp.Key);
         _ = cmd.Parameters.AddWithValue(valuepar, kvp.Value);
      }

      return cmd;
   }

   protected override void OpenConnection(SqlConnection sqlConnection) {
      SqlCommand cmd = BuildCommand(sqlConnection);
      base.OpenConnection(sqlConnection);
      _ = cmd.ExecuteNonQuery();
   }

   protected override async Task OpenConnectionAsync(SqlConnection sqlConnection, CancellationToken cancellationToken) {
      SqlCommand cmd = BuildCommand(sqlConnection);
      await base.OpenConnectionAsync(sqlConnection, cancellationToken).ConfigureAwait(false);
      _ = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
   }
}
