using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DandelifeonEmulator.Blocks
{
    public interface IBlock
    {
        Color Color { get; }

        string Name { get; }

        BlockType BlockType { get; }

        int X { get; set; }

        int Y { get; set; }

    }
}
