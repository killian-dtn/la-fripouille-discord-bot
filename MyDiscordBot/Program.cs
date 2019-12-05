using System.Configuration;

namespace MyDiscordBot
{
    class Program
    {
        static void Main(string[] args) => new MyBot(ConfigurationManager.AppSettings["Token"]).StartAsync().GetAwaiter().GetResult();
    }
}
