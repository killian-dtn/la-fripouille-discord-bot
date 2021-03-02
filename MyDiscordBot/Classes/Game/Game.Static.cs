using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MyDiscordBot;

namespace MyDiscordBot
{
    public abstract partial class Game
    {
        public static Dictionary<ulong, Player> PlayingPlayers { get; internal set; }
        public static bool IsPlaying(ulong userId) { return Game.PlayingPlayers.ContainsKey(userId); }
        public static void AddPlayingPlayer(Player player) { Game.PlayingPlayers.Add(player.Id, player); }
        public static void RemovePlayingPlayer(Player player) { Game.PlayingPlayers.Remove(player.Id); }
    }
}
