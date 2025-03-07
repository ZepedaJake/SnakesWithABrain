using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakesWithABrain.Models
{
    /// <summary>
    /// What data should be fed to the ML instance
    /// </summary>
    public class InputType
    {
        public int TypeNumber { get; set; }
        public string Description { get; set; }
    }
}
