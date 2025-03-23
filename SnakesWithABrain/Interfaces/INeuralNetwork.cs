namespace SnakesWithABrain.Interfaces
{
    public interface INeuralNetwork
    {
        /// <summary>
        /// Unique identifier for this Neural Network
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// Function to save this network to a file. Returns false by default, true if successful
        /// </summary>
        /// <returns></returns>
        public string Save() { return ""; }

        /// <summary>
        /// Rebuilds network from a string. returns true on successful load.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public bool Load(string data) { return false; }

        /// <summary>
        /// Tells the network to process inputs and give back an output
        /// </summary>
        /// <returns></returns>
        public double[] Compute(double[] inputs);
        
    }
}
