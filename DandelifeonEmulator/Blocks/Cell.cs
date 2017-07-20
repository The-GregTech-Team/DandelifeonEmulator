using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DandelifeonEmulator.Blocks
{
    class Cell : IBlock
    {
        public Color Color { get; } = Color.FromRgb(94, 217, 31);
        public string Name { get; } = "细胞";
        public BlockType BlockType { get; } = BlockType.Cell;
        public int X { get; set; }
        public int Y { get; set; }
        public int Gen { get; set; } = 0;
    }
}
