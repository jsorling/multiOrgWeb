using Sorling.SqlExec.mapper.commands;
using Sorling.sqlSessionRunner;
using System.Diagnostics;

namespace sqlSessionRunnerTests;

[TestClass]
public class SessionTests
{
   public IDictionary<string, string> SessionOrgValues { get; } = new Dictionary<string, string>(10, StringComparer.InvariantCultureIgnoreCase)
   { {"org", "123" }, {"user", "john" }, { "role","admin" } };

   [TestMethod]
   public void TestAllKeyValues() {
      SqlSessionRunner runner = new(TestsInitialize.ConnectionString, SessionOrgValues);
      string? org = runner.ExecuteScalar<string, ScriptDynamicSqlCommand>(new("select session_context(N'org')"));
      Assert.IsTrue(org == "123");

      string? user = runner.ExecuteScalar<string, ScriptDynamicSqlCommand>(new("select session_context(N'user')"));
      Assert.IsTrue(user == "john");

      string? role = runner.ExecuteScalar<string, ScriptDynamicSqlCommand>(new("select session_context(N'role')"));
      Assert.IsTrue(role == "admin");
   }

   async Task ExecManyAsync(int count, int id) {
      Stopwatch watch = Stopwatch.StartNew();

      for (int i = 0; i < count; i++) {
         SqlSessionRunner runner = new(TestsInitialize.ConnectionString, SessionOrgValues);
         string? org = await runner.ExecuteScalarAsync<string, ScriptDynamicSqlCommand>(new("select session_context(N'org')")).ConfigureAwait(false);
         Assert.IsTrue(org == "123");

         Console.WriteLine(id);
      }

      long elapsedms = watch.ElapsedMilliseconds;
      watch.Stop();
      Debug.WriteLine($"{id}-{elapsedms}");
   }

   [TestMethod]
   public void RunMany() {
      List<Task> tasklist = new();
      for (int i = 0; i < 100; i++) {
         int id = i;
         Task task = Task.Run(() => ExecManyAsync(300, id));
         tasklist.Add(task);
      }

      Task.WaitAll(tasklist.ToArray());
   }
}
