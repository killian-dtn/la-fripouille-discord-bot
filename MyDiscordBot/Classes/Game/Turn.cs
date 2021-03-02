using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDiscordBot
{
    public class Turn
    {
        public Game CurrentGame { get; private set; }
        public Player[] GamePlayers { get; private set; }
        public uint TurnNb { get; private set; }
        public uint FirstPlayer { get; private set; }

        public Player TurnOf { get { return this.GamePlayers[(this.FirstPlayer + this.TurnNb - 1) % this.GamePlayers.Length]; } }

        public Turn(Game game, params Player[] players)
        {
            this.CurrentGame = game;
            this.GamePlayers = players;
            this.TurnNb = 1;
            this.FirstPlayer = (uint)this.CurrentGame.Rnd.Next(this.GamePlayers.Length);
        }

        public void EndTurn()
        {
            this.TurnNb++;
        }
    }
}
