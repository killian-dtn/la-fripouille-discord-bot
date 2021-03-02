using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.Rest;

namespace MyDiscordBot
{
    public delegate Game GameDelegate(Game game);
    public abstract partial class Game
    {
        protected IUserMessage GameMsg { get; set; }
        protected SocketCommandContext GameContext { get; set; }
        protected Turn GameTurn { get; set; }
        protected Player[] Players { get { return this.GameTurn.GamePlayers; } }
        internal Random Rnd { get; private set; }

        protected Game(SocketCommandContext gameContext, params Player[] players)
        {
            this.GameContext = gameContext;
            this.Rnd = new Random();
            this.GameTurn = new Turn(this, players);

            foreach (Player p in this.Players)
            {
                p.CurrentGame = this;
                Game.AddPlayingPlayer(p);
            }

        }

        public virtual async Task InitGameAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
        { this.GameMsg = await this.GameContext.Channel.SendMessageAsync(text, isTTS, embed, options); }

        public virtual async Task UpdateGameAsync(Action<MessageProperties> func, RequestOptions options = null)
        {
            await this.GameMsg.ModifyAsync(func, options);
        }

        public virtual async Task EndGameAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
        {
            await this.GameContext.Channel.SendMessageAsync(text, isTTS, embed, options);
            foreach (Player p in this.Players)
                p.End();
        }
    }
}
