using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DandelifeonEmulator.Blocks;

namespace DandelifeonEmulator
{
    public class Controller
    {
        public List<Rectangle[]> Rects { get; } = new List<Rectangle[]>();
        public List<IBlock[]> Blocks { get; } = new List<IBlock[]>();

        public readonly Dictionary<Rectangle, IBlock> BlockFromRect = new Dictionary<Rectangle, IBlock>(25 * 25);

        public Rectangle GetRectangle(int x, int y) => Rects[x][y];
        public IBlock GetBlock(int x, int y) => Blocks[x][y];

        public IBlock GetBlockFromRect(Rectangle rect) => BlockFromRect[rect];
        public Rectangle GetRectFromBlock(IBlock rect) => GetRectangle(rect.X, rect.Y);

        public void UpdateBlock(IBlock newblock)
        {
            BlockFromRect[GetRectangle(newblock.X, newblock.Y)] = newblock;
            Blocks[newblock.X][newblock.Y] = newblock;
        }

        public void UpdateRect(IBlock newblock)
        {
            var rect = GetRectFromBlock(newblock);
            rect.Fill = new SolidColorBrush(newblock.Color);
            rect.ToolTip = newblock.Name;
        }

        public string Save()
        {
            var sb = new StringBuilder();
            foreach (var blocksarray in Blocks)
            {
                foreach (var block in blocksarray)
                {
                    sb.Append((int)block.BlockType);
                }
                sb.AppendLine();
            }

            return sb.ToString().GZipCompressString();
        }

        public void Load(string data)
        {
            var source = data.GZipDecompressString();
            var sp = source.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    var block = ToBlock((BlockType) int.Parse(sp[i][j].ToString()), GetBlock(i, j));
                    UpdateBlock(block);
                    UpdateRect(block);
                }
            }
            // 初始化中间3*3
            InitCenter();
        }

        public void InitCenter()
        {
            // 初始化中间3*3
            for (var i = 11; i <= 13; i++)
            {
                for (var j = 11; j <= 13; j++)
                {
                    var rec = GetRectangle(i, j);
                    if (!(i == 12 && j == 12))
                    {
                        rec.Fill = new SolidColorBrush(Yellow);
                        rec.ToolTip = "别动！那里放东西可没什么意义！";
                    }
                    else
                    {
                        rec.Fill = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Dandelifeon.png")));
                        var fl = new Dandelifeon { X = i, Y = j };
                        UpdateBlock(fl);
                        GetRectFromBlock(fl).ToolTip = fl.Name;
                    }
                }
            }
        }

        private static readonly Color Yellow = Color.FromRgb(255, 221, 20);

        public static IBlock ToBlock(BlockType type, IBlock blockbase)
        {
            var block = blockbase ?? new Air();
            switch (type)
            {
                case BlockType.Air:
                    return new Air { X = block.X, Y = block.Y };
                case BlockType.Cell:
                    return new Cell { X = block.X, Y = block.Y };
                case BlockType.Stone:
                    return new Stone { X = block.X, Y = block.Y };
                case BlockType.Dandelifeon:
                    return new Dandelifeon { X = block.X, Y = block.Y };
                default:
                    return null;
            }
        }

        private const int Range = 12;
        private const int MaxGenerations = 60;
        private const int ManaPerGen = 150;

        public void Run()
        {
            var table = GetCellTable();
            var changes = new List<int[]>();
            var wipe = false;

            for (var i = 0; i < table.Count; i++)
            {
                for (var j = 0; j < table[0].Length; j++)
                {
                    var gen = table[i][j];
                    int adj = GetAdjCells(table, i, j);
                    int newVal = gen;
                    if (adj < 2 || adj > 3)
                        newVal = -1;
                    else
                    {
                        if (adj == 3 && gen == -1)
                            newVal = GetSpawnCellGeneration(table, i, j);
                        else if (gen > -1)
                            newVal = gen + 1;
                    }
                    int xdist = Math.Abs(i - Range);
                    int zdist = Math.Abs(j - Range);
                    int allowDist = 1;
                    if (xdist <= allowDist && zdist <= allowDist && newVal > -1)
                    {
                        gen = newVal;
                        newVal = gen == 1 ? -1 : -2;
                    }

                    if (newVal != gen)
                    {
                        changes.Add(new int[] { i, j, newVal, gen });
                        if (newVal == -2)
                            wipe = true;
                    }

                }
            }
            foreach (var change in changes)
            {
                int px = 12 - Range + change[0];
                int pz = 12 - Range + change[1];
                int val = change[2];
                if (val != -2 && wipe)
                    val = -1;

                int old = change[3];

                SetBlockForGeneration(px, pz, val, old);
            }

        }


        public bool Isfinal;
        public int Mana;
        void SetBlockForGeneration(int x, int z, int gen, int prevGen)
        {
            var tile = GetBlock(x, z);

            if (gen == -2)
            {
                int val = prevGen * ManaPerGen;
                Mana += Math.Min(val, 50000);
                Isfinal = true;
            }
            else if (tile is Cell cell)
            {
                if (gen < 0 || gen > MaxGenerations)
                {
                    var newblock = ToBlock(BlockType.Air, tile);
                    UpdateBlock(newblock);
                    UpdateRect(newblock);
                }
                else
                {
                    cell.Gen = gen;
                }
            }
            else if (gen >= 0 && tile is Air)
            {
                var newblock = (Cell)ToBlock(BlockType.Cell, tile);
                newblock.Gen = gen;
                UpdateBlock(newblock);
                UpdateRect(newblock);
            }

        }

        private static readonly int[][] AdjacentBlocks = {
            new int[] { -1, -1 }, new int[] { -1, +0 },
            new int[] { -1, +1 }, new int[] { +0, +1 },
            new int[] { +1, +1 }, new int[] { +1, +0 },
            new int[] { +1, -1 }, new int[] { +0, -1 }
        };

        private int GetCellGeneration(int x, int y)
        {
            var block = GetBlock(x, y);
            if (block is Cell cell)
            {
                return cell.Gen;
            }
            return -1;
        }

        private List<int[]> GetCellTable()
        {
            var list = new List<int[]>(25);
            for (var i = 0; i < 25; i++) list.Add(new int[25]);
            const int diam = 12 * 2 + 1;
            for (var i = 0; i < diam; i++)
            {
                for (var j = 0; j < diam; j++)
                {
                    list[i][j] = GetCellGeneration(i, j);
                }
            }
            return list;
        }

        bool IsOffBounds(List<int[]> table, int x, int z)
        {
            return x < 0 || z < 0 || x >= table.Count || z >= table[0].Length;
        }

        int GetAdjCells(List<int[]> table, int x, int z)
        {
            return (from shift in AdjacentBlocks let xp = x + shift[0] let zp = z + shift[1] where !IsOffBounds(table, xp, zp) select table[xp][zp]).Count(gen => gen >= 0);
        }

        int GetSpawnCellGeneration(List<int[]> table, int x, int z)
        {
            return (from shift in AdjacentBlocks let xp = x + shift[0] let zp = z + shift[1] where !IsOffBounds(table, xp, zp) select table[xp][zp]).Concat(new[] { -1 }).Max() == -1 ? -1 : (from shift in AdjacentBlocks let xp = x + shift[0] let zp = z + shift[1] where !IsOffBounds(table, xp, zp) select table[xp][zp]).Concat(new[] { -1 }).Max() + 1;
        }
    }
}
