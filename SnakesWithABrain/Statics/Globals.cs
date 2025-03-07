using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakesWithABrain.Statics
{
    /// <summary>
    /// Things I want everything to be able to access, but do not necessarily want to make statics themselves.
    /// </summary>
    public static class Globals
    {
        /// <summary>
        /// Instance of current training session. Global so that snakes can reference it for food position, board size, and wrap rule.
        /// </summary>
        public static TrainingSession CurrentTrainingSession = null;

    }
}
