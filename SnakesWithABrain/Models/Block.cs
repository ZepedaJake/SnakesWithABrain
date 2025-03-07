using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakesWithABrain
{
    public class Block
    {
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;

        public Block(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Block()
        {

        }
    }
}
