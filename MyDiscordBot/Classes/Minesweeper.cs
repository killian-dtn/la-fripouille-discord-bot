using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDiscordBot
{
    public class Minesweeper : Game
    {
        private ushort Size { get; set; }
        private short[][] Map { get; set; }
        private Minesweeper.Mask[][] MapMask { get; set; }
        private ushort NbBombs { get; set; }
        private ushort NbFlags { get; set; }
        private bool ContainsError { get; set; }
        private uint VisibleTilesNb { get; set; }
        private Player Player
        {
            get => this.Players[0];
            set { this.Players[0] = value; }
        }

        public Minesweeper(SocketCommandContext gameContext, Player player, ushort size = 10, ushort bombs = 20) : base(gameContext, player)
        {
            this.Size = size;
            this.Map = new short[this.Size][];
            this.MapMask = new Mask[this.Size][];
            this.NbBombs = bombs;
            this.NbFlags = 0;
            this.ContainsError = false;
            this.VisibleTilesNb = 0;

            if (this.Size > 26) this.Size = 26;

            for (ushort i = 0; i < this.Size; i++)
            {
                this.Map[i] = new short[this.Size];
                this.MapMask[i] = new Mask[this.Size];
            }

            this.InitMap();
        }

        public string GenerateMessage()
        {
            string msg = " ";

            for (int i = -1; i < this.Size; i++)
            {
                for (int j = 0; j <= this.Size; j++)
                    if (i < 0 && j < this.Size)
                        msg += Minesweeper.NumberToLetter((ushort)j) + (j % 2 == 0 ? " " : "  ");
                    else if (i > -1 && j < this.Size)
                        msg += $"{this.CoordsToEmote((ushort)i, (ushort)j)}";
                    else if (i > -1 && j == this.Size)
                        msg += Minesweeper.NumberToLetter((ushort)i);
                msg += "\n";
            }

            //Console.WriteLine(($"```{msg}```\n\uD83D\uDEA9 : {this.NbFlags} / {this.NbBombs}").Length);
            return $"```{msg}```\n\uD83D\uDEA9 : {this.NbFlags} / {this.NbBombs}";
        }

        public void Sweep(string coords)
        {
            List<int[]> coordsCheck = new List<int[]>() { Minesweeper.StringCoordsToIndex(coords) };
            int[] c = coordsCheck[0];

            if (c[0] < 0 || c[0] > this.Size || c[1] < 0 || c[1] > this.Size)
                return;

            if (this.MapMask[c[0]][c[1]] == Minesweeper.Mask.Flag || this.MapMask[c[0]][c[1]] == Minesweeper.Mask.Unknown)
                return;
            else if (this.MapMask[c[0]][c[1]] == Minesweeper.Mask.Visible && this.Map[c[0]][c[1]] > 0)
            {
                int flagsNearby = 0;
                List<int[]> tmpCoordsCheck = new List<int[]>();

                for (int i = -1; i < 2; i++)
                    for (int j = -1; j < 2; j++)
                        if (!(i == 0 && j == 0))
                            if (c[0] + i >= 0 && c[0] + i < this.Size && c[1] + j >= 0 && c[1] + j < this.Size)
                                if (this.MapMask[c[0] + i][c[1] + j] == Minesweeper.Mask.Flag)
                                    flagsNearby++;
                                else if (this.MapMask[c[0] + i][c[1] + j] == Minesweeper.Mask.Hidden)
                                    tmpCoordsCheck.Add(new int[] { c[0] + i, c[1] + j });

                if (flagsNearby == this.Map[c[0]][c[1]])
                    coordsCheck.AddRange(tmpCoordsCheck);

                coordsCheck.RemoveAt(0);
            }

            while (coordsCheck.Count > 0)
            {
                c = coordsCheck[0];

                if (this.Map[c[0]][c[1]] == -1)
                {
                    for (int i = 0; i < this.Size; i++)
                        for (int j = 0; j < this.Size; j++)
                            if (this.MapMask[i][j] == Minesweeper.Mask.Flag && this.Map[i][j] > -1)
                                this.MapMask[i][j] = Minesweeper.Mask.Error;
                            else
                                this.MapMask[i][j] = Minesweeper.Mask.Visible;

                    this.MapMask[c[0]][c[1]] = Minesweeper.Mask.Explode;
                    this.ContainsError |= true;

                    return;
                }
                else if (this.Map[c[0]][c[1]] == 0)
                    for (int i = -1; i < 2; i++)
                        for (int j = -1; j < 2; j++)
                            if (!(i == 0 && j == 0))
                                if ((c[0] + i >= 0 && c[0] + i < this.Size) && (c[1] + j >= 0 && c[1] + j < this.Size))
                                    if ((!coordsCheck.Exists(x => x[0] == c[0] + i && x[1] == c[1] + j)) && this.MapMask[c[0] + i][c[1] + j] == Minesweeper.Mask.Hidden)
                                        coordsCheck.Add(new int[] { c[0] + i, c[1] + j });
                
                if (this.Map[c[0]][c[1]] >= 0)
                {
                    this.MapMask[c[0]][c[1]] = Minesweeper.Mask.Visible;
                    this.VisibleTilesNb++;
                    coordsCheck.RemoveAt(0);
                }
            }
        }

        public void SetFlag(string coords) { this.SetMask(coords, Minesweeper.Mask.Flag); }

        public void SetUnknown(string coords) { this.SetMask(coords, Minesweeper.Mask.Unknown); }

        private void SetMask(string coords, Minesweeper.Mask mask)
        {
            int[] nbCoords = Minesweeper.StringCoordsToIndex(coords);

            if (nbCoords != null && nbCoords[0] < this.Size && nbCoords[0] >= 0 && nbCoords[1] >= 0 && nbCoords[1] < this.Size)
                if (this.MapMask[nbCoords[0]][nbCoords[1]] == Minesweeper.Mask.Hidden)
                {
                    if (mask == Minesweeper.Mask.Flag)
                        this.NbFlags++;
                    this.MapMask[nbCoords[0]][nbCoords[1]] = mask;
                }
                else if (this.MapMask[nbCoords[0]][nbCoords[1]] != Minesweeper.Mask.Hidden && this.MapMask[nbCoords[0]][nbCoords[1]] != Minesweeper.Mask.Visible)
                {
                    if (mask == Minesweeper.Mask.Flag)
                        this.NbFlags--;
                    this.MapMask[nbCoords[0]][nbCoords[1]] = Minesweeper.Mask.Hidden;
                }
        }

        public bool IsWin() { return this.VisibleTilesNb == (Math.Pow(this.Size, 2) - this.NbBombs); }

        public bool IsLose() { return this.ContainsError; }

        private void InitMap()
        {
            if (this.NbBombs > Math.Pow(this.Size, 2)) return;

            List<ushort[]> indexes = new List<ushort[]>();

            for (ushort i = 0; i < this.Size; i++)
                for (ushort j = 0; j < this.Size; j++)
                    indexes.Add(new ushort[] { i, j });

            for (ushort i = 0; i < this.NbBombs; i++)
            {
                int index = this.Rnd.Next(indexes.Count);
                ushort[] bomb = indexes[index];
                indexes.RemoveAt(index);

                //Place bombs
                this.Map[bomb[0]][bomb[1]] = -1;

                //Increment numbers
                for (int j = -1; j < 2; j++)
                    for (int k = -1; k < 2; k++)
                        if (!(j == 0 && k == 0))
                            if ((bomb[0] + j >= 0 && bomb[0] + j < this.Size) && (bomb[1] + k >= 0 && bomb[1] + k < this.Size))
                                if (this.Map[bomb[0] + j][bomb[1] + k] > -1)
                                    this.Map[bomb[0] + j][bomb[1] + k]++;
            }
        }

        private static int[] StringCoordsToIndex(string coords)
        {
            if (string.IsNullOrEmpty(coords) || coords.Length > 2)
                return null;

            char[] charCoords = coords.ToCharArray(0, 2);
            int[] nbCoords = new int[2];

            for (int i = 0; i < 2; i++)
                if (char.IsLetter(charCoords[i]))
                    nbCoords[i] = (int)(charCoords[i] % 32) - 1;

            return nbCoords;
        }

        private static char NumberToLetter(ushort nb)
        {
            if (nb > 25) return '\0';
            char[] letters = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
            return letters[nb];
        }

        private string CoordsToEmote(ushort x, ushort y)
        {
            if (x >= this.Size || y >= this.Size) return null;

            if (this.MapMask[x][y] == Minesweeper.Mask.Visible)
                switch (this.Map[x][y])
                {
                    case -1:
                        return "\uD83D\uDCA3";
                    case 0:
                        return "\uD83D\uDFE6";
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                        return $"{this.Map[x][y]}\uFE0F\u20E3";
                }
            else
                switch (this.MapMask[x][y])
                {
                    case Minesweeper.Mask.Flag:
                        return "\uD83D\uDEA9";
                    case Minesweeper.Mask.Unknown:
                        return "\u2754";
                    case Minesweeper.Mask.Explode:
                        return "\uD83D\uDCA5";
                    case Minesweeper.Mask.Error:
                        return "\u274C";
                    default:
                        return "\u2B1B";
                }

            return null;
        }

        private enum Mask
        {
            Flag = -4,
            Unknown,
            Explode,
            Error,
            Hidden,
            Visible
        }
    }
}
