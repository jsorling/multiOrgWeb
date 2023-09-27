using Microsoft.Extensions.Configuration;

namespace sqlSessionRunnerTests;

[TestClass]
public class TestsInitialize
{
   private static IConfigurationSection InitWithUserSecrets<T>(string section) where T : class
      => new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
         .AddJsonFile("appsettings.json", true, false).AddUserSecrets<T>().Build().GetSection(section);

   public static string ConnectionString { get; private set; } = "";

   [AssemblyInitialize]
#pragma warning disable IDE0060 // Remove unused parameter
   public static void AssemblyInitialize(TestContext testContext) {
#pragma warning restore IDE0060 // Remove unused parameter
      IConfigurationSection conf = InitWithUserSecrets<TestsInitialize>("Test");
      ConnectionString = conf["ConnectionString"]
         ?? throw new ApplicationException("ConnectionString not set in configuration");
   }
}
