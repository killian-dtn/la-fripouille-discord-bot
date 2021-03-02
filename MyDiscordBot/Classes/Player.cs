using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace MyDiscordBot
{
    public class Player
    {
        internal IUser User { get; set; }
        public Game CurrentGame { get; internal set; }
        internal ulong Id { get { return this.User.Id; } }

        public Player(IUser user) { this.User = user; }

        public void Play(GameDelegate move) { move(this.CurrentGame); }
        public void End() { Game.RemovePlayingPlayer(this); }
    }
}
