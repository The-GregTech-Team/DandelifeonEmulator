using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DandelifeonEmulator.Blocks
{
    class Dandelifeon : IBlock
    {
        public Color Color { get; } = new Color();
        public string Name { get; } = "启命英";
        public BlockType BlockType { get; } = BlockType.Dandelifeon;
        public int X { get; set; }
        public int Y { get; set; }
    }
}
