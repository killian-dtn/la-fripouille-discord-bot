using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using DiscordBotBase;
using MyDiscordBot.Modules;

namespace MyDiscordBot
{
    public class MyBot : BasicBot
    {
        public MyBot(string token) : base(token)
        {
            Game.PlayingPlayers = new Dictionary<ulong, Player>();
            this.Commands.AddModulesAsync(assembly: Assembly.GetAssembly(typeof(MinesweeperModule)), services: null);
        }
    }
}
