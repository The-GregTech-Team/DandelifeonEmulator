using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DandelifeonEmulator.Blocks
{
    class Stone : IBlock
    {
        public Color Color { get; } = Color.FromRgb(73, 73, 75);
        public string Name { get; } = "阻挡方块";
        public BlockType BlockType { get; } = BlockType.Stone;
        public int X { get; set; }
        public int Y { get; set; }
    }
}
