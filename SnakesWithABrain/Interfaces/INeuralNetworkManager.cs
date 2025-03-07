using SnakesWithABrain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakesWithABrain.Interfaces
{
    /// <summary>
    /// Performs functions on neural networks.
    /// </summary>   
    public interface INeuralNetworkManager
    {
        /// <summary>
        /// How many inputs will these neural networks have?
        /// </summary>
        public int Inputs { get; set; }

        /// <summary>
        /// How many outputs will these neural networks have?
        /// </summary>
        public int Outputs { get; set; }
        /// <summary>
        /// Returns a clean slate version of an INeuralNetwork
        /// </summary>
        public INeuralNetwork FreshNeuralNetwork();

        /// <summary>
        /// Returns an INeuralNetwork that has some data copied from the original. Sometimes you may want a slight copy without keeping a direct reference.
        /// </summary>
        /// <returns></returns>
        public INeuralNetwork SoftCopy(INeuralNetwork copyMe);

        /// <summary>
        /// Returns an exact copy of an INeuralNetwork. Sometimes you may want a copy without keeping a direct reference.
        /// </summary>
        public INeuralNetwork HardCopy(INeuralNetwork copyMe);

        /// <summary>
        /// Creates new snakes by breeding some of the best ones.
        /// </summary>
        public INeuralNetwork Breed(INeuralNetwork parentA, INeuralNetwork parentB);

        /// <summary>
        /// Creates new snakes by mutating some of the best ones.
        /// </summary>
        public INeuralNetwork Mutate(INeuralNetwork mutateMe);
    }
}
