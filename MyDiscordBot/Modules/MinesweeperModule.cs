using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace MyDiscordBot.Modules
{
    [Name("Minesweeper")]
    public class MinesweeperModule : ModuleBase<SocketCommandContext>
    {
        [Command("msstart"), Alias("msst")]
        public async Task StartAsync(ushort size = 10, ushort bombs = 20)
        {
            if (!Game.IsPlaying(this.Context.User.Id))
            {
                Minesweeper ms = new Minesweeper(this.Context, new Player(this.Context.User), size, bombs);

                await ms.InitGameAsync(ms.GenerateMessage() + $" {this.Context.User.Mention}");
            }

            await this.Context.Message.DeleteAsync();
        }

        [Command("mssweep"), Alias("mssw")]
        public async Task SweepAsync(string coords)
        {
            if (Game.IsPlaying(this.Context.User.Id))
            {
                Player p = Game.PlayingPlayers[this.Context.User.Id];
                p.Play(game =>
                {
                    ((Minesweeper)game).Sweep(coords);
                    return game;
                });

                await p.CurrentGame.UpdateGameAsync(msg => msg.Content = ((Minesweeper)(p.CurrentGame)).GenerateMessage() + $" {this.Context.User.Mention}");

                if (((Minesweeper)(p.CurrentGame)).IsLose())
                    await p.CurrentGame.EndGameAsync($"Perdu {this.Context.User.Mention} !");
                else if (((Minesweeper)(p.CurrentGame)).IsWin())
                    await p.CurrentGame.EndGameAsync($"Gagné {this.Context.User.Mention} !");
                
                await this.Context.Message.DeleteAsync();
            }
        }

        [Command("msflag"), Alias("msf")]
        public async Task FlagAsync(params string[] coords)
        {
            if (Game.IsPlaying(this.Context.User.Id))
            {
                Player p = Game.PlayingPlayers[this.Context.User.Id];
                p.Play(game =>
                {
                    foreach (string c in coords)
                        ((Minesweeper)game).SetFlag(c);
                    return (Minesweeper)game;
                });

                await p.CurrentGame.UpdateGameAsync(msg => msg.Content = ((Minesweeper)(p.CurrentGame)).GenerateMessage() + $" {this.Context.User.Mention}");
            }
            
            await this.Context.Message.DeleteAsync();
        }

        [Command("msend"), Alias("mse")]
        public async Task EndAsync()
        {
            if (Game.IsPlaying(this.Context.User.Id))
                await Game.PlayingPlayers[this.Context.User.Id].CurrentGame.EndGameAsync($"Abandon de {this.Context.User.Mention}... GROS NULLOS");
            
            await this.Context.Message.DeleteAsync();
        }
    }
}
