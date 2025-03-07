using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakesWithABrain.Models
{
    public class Replay
    {
        public string GUID = "";
        public int Generation = 1;
        //public int SnakeIndex = 1;
        public string TrainingGUID = "";//which training session was this replay from.
        public string SnakeGUID = "";//instance of sn
        public int StartX = 0;
        public int StartY = 0;
        public List<Block> FoodLocationsInOrder = new List<Block>();//order that food spawned in
    }
}
