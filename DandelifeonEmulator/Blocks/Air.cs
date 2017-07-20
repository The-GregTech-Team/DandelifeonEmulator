using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DandelifeonEmulator.Blocks
{
    class Air : IBlock
    {
        public Color Color { get; } = Color.FromRgb(255, 255, 255);
        public string Name { get; } = "空气";
        public BlockType BlockType { get; } = BlockType.Air;
        public int X { get; set; }
        public int Y { get; set; }
    }
}
