using System;
using SnakesWithABrain.Enums;
using SnakesWithABrain.Interfaces;

namespace SnakesWithABrain
{
    public class LSTMCell:INeuralNetwork
    {        
        //for example's sake input is 3 long output is 5
        //only weights need to be multi dimensional, everything else can be 1 dimensional
        //everything except the inputs will be 1 x output size
        //input weights size output x input, [5,3]
        public double[,] weightsFin;
        public double[,] weightsIin;
        public double[,] weightsOin;
        public double[,] weightsCin;

        //output weights size output x output [5,5]
        public double[,] weightsFout;
        public double[,] weightsIout;
        public double[,] weightsOout;
        public double[,] weightsCout;

        //bias' size output [5]
        public double[] biasf;
        public double[] biasi;
        public double[] biaso;
        public double[] biasc;


        public double[] cellState;
        public double[] lastCellState;
        public double[] output;
        public double[] lastOutput;

        double[] input;

        public int inputSize;
        public int outputSize;

        public double[] forgetGate;
        public double[] inputGate;
        public double[] outputGate;
        public double[] cellGate;

        Random random = new Random();
        public string Guid { get; set; } = "";
        public LSTMCell(string loadMe)
        {
            string[] lines = loadMe.Split('\n');
            int targetIndex = Array.IndexOf(lines, "[Guid]") + 1;//line after identifier is the info
            Guid = lines[targetIndex];

            targetIndex = Array.IndexOf(lines, "[InputSize]") + 1;
            inputSize = int.Parse(lines[targetIndex]);

            targetIndex = Array.IndexOf(lines, "[OutputSize]") + 1;
            outputSize = int.Parse(lines[targetIndex]);


            //set up array sizes
            input = new double[inputSize];//fed in
            output = new double[outputSize]; //computed
            lastOutput = new double[outputSize]; //computed

            cellState = new double[outputSize]; //computed
            lastCellState = new double[outputSize]; //computed

            forgetGate = new double[outputSize]; //computed
            inputGate = new double[inputSize];//computed
            outputGate = new double[outputSize];//computed
            cellGate = new double[outputSize];//computed

            biasf = new double[outputSize];//Generated / static
            biasi = new double[outputSize];//Generated / static
            biasc = new double[outputSize];//Generated / static
            biaso = new double[outputSize];//Generated / static

            weightsFin = new double[outputSize, inputSize];//Generated / static
            weightsIin = new double[outputSize, inputSize];//Generated / static
            weightsCin = new double[outputSize, inputSize];//Generated / static
            weightsOin = new double[outputSize, inputSize];//Generated / static

            weightsFout = new double[outputSize, outputSize];//Generated / static
            weightsIout = new double[outputSize, outputSize];//Generated / static
            weightsCout = new double[outputSize, outputSize];//Generated / static
            weightsOout = new double[outputSize, outputSize];//Generated / static

            targetIndex = Array.IndexOf(lines, "[WeightsFIn]") + 1;
            for (int y = 0; y < weightsFin.GetLength(1); y++)
            {
                string[] thisLine = lines[targetIndex + y].Split(',');

                for (int x = 0; x < weightsFin.GetLength(0); x++)
                {
                    weightsFin[x, y] = double.Parse(thisLine[x]);
                }
            }

            targetIndex = Array.IndexOf(lines, "[WeightsFOut]") + 1;
            for (int y = 0; y < weightsFout.GetLength(1); y++)
            {
                string[] thisLine = lines[targetIndex + y].Split(',');

                for (int x = 0; x < weightsFout.GetLength(0); x++)
                {
                    weightsFout[x, y] = double.Parse(thisLine[x]);
                }
            }

            targetIndex = Array.IndexOf(lines, "[BiasF]") + 1;
            string[] biasLine = lines[targetIndex].Split(',');
            for (int x = 0; x < biasf.GetLength(0); x++)
            {                
                biasf[x] = double.Parse(biasLine[x]);
            }


            targetIndex = Array.IndexOf(lines, "[WeightsIIn]") + 1;
            for (int y = 0; y < weightsIin.GetLength(1); y++)
            {
                string[] thisLine = lines[targetIndex + y].Split(',');

                for (int x = 0; x < weightsIin.GetLength(0); x++)
                {
                    weightsIin[x, y] = double.Parse(thisLine[x]);
                }
            }

            targetIndex = Array.IndexOf(lines, "[WeightsIOut]") + 1;
            for (int y = 0; y < weightsIout.GetLength(1); y++)
            {
                string[] thisLine = lines[targetIndex + y].Split(',');

                for (int x = 0; x < weightsIout.GetLength(0); x++)
                {
                    weightsIout[x, y] = double.Parse(thisLine[x]);
                }
            }

            targetIndex = Array.IndexOf(lines, "[BiasI]") + 1;
            biasLine = lines[targetIndex].Split(',');
            for (int x = 0; x < biasi.GetLength(0); x++)
            {               
                biasi[x] = double.Parse(biasLine[x]);
            }


            targetIndex = Array.IndexOf(lines, "[WeightsOIn]") + 1;
            for (int y = 0; y < weightsOin.GetLength(1); y++)
            {
                string[] thisLine = lines[targetIndex + y].Split(',');

                for (int x = 0; x < weightsOin.GetLength(0); x++)
                {
                    weightsOin[x, y] = double.Parse(thisLine[x]);
                }
            }

            targetIndex = Array.IndexOf(lines, "[WeightsOOut]") + 1;
            for (int y = 0; y < weightsOout.GetLength(1); y++)
            {
                string[] thisLine = lines[targetIndex + y].Split(',');

                for (int x = 0; x < weightsOout.GetLength(0); x++)
                {
                    weightsOout[x, y] = double.Parse(thisLine[x]);
                }
            }

            targetIndex = Array.IndexOf(lines, "[BiasO]") + 1;
            biasLine = lines[targetIndex].Split(',');
            for (int x = 0; x < biaso.GetLength(0); x++)
            {                
                biaso[x] = double.Parse(biasLine[x]);
            }


            targetIndex = Array.IndexOf(lines, "[WeightsCIn]") + 1;
            for (int y = 0; y < weightsCin.GetLength(1); y++)
            {
                string[] thisLine = lines[targetIndex + y].Split(',');

                for (int x = 0; x < weightsCin.GetLength(0); x++)
                {
                    weightsCin[x, y] = double.Parse(thisLine[x]);
                }
            }

            targetIndex = Array.IndexOf(lines, "[WeightsCOut]") + 1;
            for (int y = 0; y < weightsCout.GetLength(1); y++)
            {
                string[] thisLine = lines[targetIndex + y].Split(',');

                for (int x = 0; x < weightsCout.GetLength(0); x++)
                {
                    weightsCout[x, y] = double.Parse(thisLine[x]);
                }
            }

            targetIndex = Array.IndexOf(lines, "[BiasC]") + 1;
            biasLine = lines[targetIndex].Split(',');
            for (int x = 0; x < biasc.GetLength(0); x++)
            {                
                biasc[x] = double.Parse(biasLine[x]);
            }


        }
        public LSTMCell(int inputLength, int outputLength)
        {
            Guid = System.Guid.NewGuid().ToString();
            inputSize = inputLength;
            outputSize = outputLength;
            input = new double[inputSize];//fed in

            output = new double[outputSize]; //computed
            lastOutput = new double[outputSize]; //computed

            cellState = new double[outputSize]; //computed
            lastCellState = new double[outputSize]; //computed


            forgetGate = new double[outputSize]; //computed
            inputGate = new double[inputSize];//computed
            outputGate = new double[outputSize];//computed
            cellGate = new double[outputSize];//computed

            biasf = new double[outputSize];//Generated / static
            biasi = new double[outputSize];//Generated / static
            biasc = new double[outputSize];//Generated / static
            biaso = new double[outputSize];//Generated / static

            weightsFin = new double[outputSize, inputSize];//Generated / static
            weightsIin = new double[outputSize, inputSize];//Generated / static
            weightsCin = new double[outputSize, inputSize];//Generated / static
            weightsOin = new double[outputSize, inputSize];//Generated / static

            weightsFout = new double[outputSize, outputSize];//Generated / static
            weightsIout = new double[outputSize, outputSize];//Generated / static
            weightsCout = new double[outputSize, outputSize];//Generated / static
            weightsOout = new double[outputSize, outputSize];//Generated / static
            InitWeights();

            //Coming back to this after over a year, it looks like i may be mistakenly using separate weight for gates such that...
            //instead of Sig(Wi[ht-1,xt] + bi) I have Sig([Wiin * ht-1] + [Wiout * xt] + bi)
            //basically creating weights per input/output instead of per function/gate
            //I have no idea why i did this or how i came to the conclusion that it is correct..
            //This appears incorrect based on this information: https://colah.github.io/posts/2015-08-Understanding-LSTMs/, but does seem to work still so i will keep it

            //step 1, forgetGate, Sig(weightsFin*inputs + weightFout*lastOutput + biasf) 
            //step 2, inputGate, Sig(weightsIin*inputs + weightIout*lastOutput + biasi) 
            //step 3, tanh input, TanH(weightscIn*inputs + weightsCout*lastOutput + biasc)
            //step 4, outputGate, Sig(weightsOin*inputs + weightOout*lastOutput + biaso)
            //step 5, element mult forget, Hada(forgetGate, lastCellState)
            //step 6, element mult input, Hada(inputgate, 'step 3')
            //step 7 this gets the new cell state, element add, Add(step5, step6) 
            //step 8, tanh cell state, Tanh(cellState)
            //step 9, output, Hada(outputGate, cellState)        


        }

        void InitWeights()
        {
            //doing *2 in here just for larger weights
            for (int x = 0; x < outputSize; x++)
            {
                for (int y = 0; y < inputSize; y++)
                {
                    try
                    {
                        weightsFin[x, y] = ((double)random.NextDouble() - 0.5f) * 2;
                        weightsIin[x, y] = ((double)random.NextDouble() - 0.5f) * 2;
                        weightsOin[x, y] = ((double)random.NextDouble() - 0.5f) * 2;
                        weightsCin[x, y] = ((double)random.NextDouble() - 0.5f) * 2;
                    }
                    catch
                    {
                        //MessageBox.Show(x + ", " + y + " | " + weightsFin.GetLongLength(0) + ", " + weightsFin.GetLongLength(1) + "\nInputGateLength: " + inputGate.Length);
                    }
                }

                for (int z = 0; z < outputSize; z++)
                {
                    weightsFout[x, z] = ((double)random.NextDouble() - 0.5f) * 2;
                    weightsIout[x, z] = ((double)random.NextDouble() - 0.5f) * 2;
                    weightsOout[x, z] = ((double)random.NextDouble() - 0.5f) * 2;
                    weightsCout[x, z] = ((double)random.NextDouble() - 0.5f) * 2;
                }

                biasf[x] = ((double)random.NextDouble() - 0.5f) * 2;
                biasi[x] = ((double)random.NextDouble() - 0.5f) * 2;
                biasc[x] = ((double)random.NextDouble() - 0.5f) * 2;
                biaso[x] = ((double)random.NextDouble() - 0.5f) * 2;
            }
        }

        /// <summary>
        /// Uses input array to determine an output.
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public double[] Compute(double[] inputs)
        {
            input = inputs;
            lastCellState = cellState;
            lastOutput = output;

            forgetGate = MoreMath.MatrixSigmoid(MoreMath.MatrixAdd(MoreMath.MatrixAdd(MoreMath.MatrixMultiply(weightsFin, input), MoreMath.MatrixMultiply(weightsFout, lastOutput)), biasf));
            inputGate = MoreMath.MatrixSigmoid(MoreMath.MatrixAdd(MoreMath.MatrixAdd(MoreMath.MatrixMultiply(weightsIin, input), MoreMath.MatrixMultiply(weightsIout, lastOutput)), biasi));
            outputGate = MoreMath.MatrixSigmoid(MoreMath.MatrixAdd(MoreMath.MatrixAdd(MoreMath.MatrixMultiply(weightsOin, input), MoreMath.MatrixMultiply(weightsOout, lastOutput)), biaso));
            cellGate = MoreMath.MatrixTanh(MoreMath.MatrixAdd(MoreMath.MatrixAdd(MoreMath.MatrixMultiply(weightsCin, input), MoreMath.MatrixMultiply(weightsCout, lastOutput)), biasc));

            cellState = MoreMath.MatrixAdd(MoreMath.MatrixHadamard(forgetGate, lastCellState), MoreMath.MatrixHadamard(inputGate, cellGate));

            output = MoreMath.MatrixHadamard(outputGate, MoreMath.MatrixTanh(cellState));
            return output;
        }


        public double[,] CopyArray(double[,] c)//2 dimensional
        {
            double[,] copied = new double[c.GetLongLength(0), c.GetLongLength(1)];
            for (int a = 0; a < c.GetLongLength(0); a++)
            {
                for (int b = 0; b < c.GetLongLength(1); b++)
                {
                    copied[a, b] = c[a, b];
                }
            }

            return copied;
        }

        public double[] CopyArray(double[] c)//1 dimensional
        {
            double[] copied = new double[c.Length];
            for (int a = 0; a < c.Length; a++)
            {
                copied[a] = c[a];
            }

            return copied;
        }

        public string Save()
        {
            string returnMe = "";
            returnMe += $"[GUID]\n{Guid}\n";
            returnMe += $"[InputSize]\n{inputSize.ToString()}\n";
            returnMe += $"[OutputSize]\n{outputSize}\n";
            returnMe += "[WeightsFIn]\n";
            for (int y = 0; y < weightsFin.GetLength(1); y++)
            {
                for (int x = 0; x < weightsFin.GetLength(0); x++)
                {
                    returnMe += weightsFin[x, y] + ",";
                }
                returnMe = returnMe.Remove(returnMe.Length - 1);
                returnMe += "\n";
            }

            returnMe += "[WeightsFOut]\n";
            for (int y = 0; y < weightsFout.GetLength(1); y++)
            {
                for (int x = 0; x < weightsFout.GetLength(0); x++)
                {
                    returnMe += weightsFout[x, y] + ",";
                }
                returnMe = returnMe.Remove(returnMe.Length - 1);
                returnMe += "\n";
            }

            returnMe += "[BiasF]\n";//bias is 1 dimension so join works fine
            returnMe += string.Join(",", biasf) + "\n";

            returnMe += "[WeightsIIn]\n";
            for (int y = 0; y < weightsIin.GetLength(1); y++)
            {
                for (int x = 0; x < weightsIin.GetLength(0); x++)
                {
                    returnMe += weightsIin[x, y] + ",";
                }
                returnMe = returnMe.Remove(returnMe.Length - 1);
                returnMe += "\n";
            }

            returnMe += "[WeightsIOut]\n";
            for (int y = 0; y < weightsIout.GetLength(1); y++)
            {
                for (int x = 0; x < weightsIout.GetLength(0); x++)
                {
                    returnMe += weightsIout[x, y] + ",";
                }
                returnMe = returnMe.Remove(returnMe.Length - 1);
                returnMe += "\n";
            }

            returnMe += "[BiasI]\n";
            returnMe += string.Join(",", biasi) + "\n";

            returnMe += "[WeightsCIn]\n";
            for (int y = 0; y < weightsCin.GetLength(1); y++)
            {
                for (int x = 0; x < weightsCin.GetLength(0); x++)
                {
                    returnMe += weightsCin[x, y] + ",";
                }
                returnMe = returnMe.Remove(returnMe.Length - 1);
                returnMe += "\n";
            }

            returnMe += "[WeightsCOut]\n";
            for (int y = 0; y < weightsCout.GetLength(1); y++)
            {
                for (int x = 0; x < weightsCout.GetLength(0); x++)
                {
                    returnMe += weightsCout[x, y] + ",";
                }
                returnMe = returnMe.Remove(returnMe.Length - 1);
                returnMe += "\n";
            }

            returnMe += "[BiasC]\n";
            returnMe += string.Join(",", biasc) + "\n";

            returnMe += "[WeightsOIn]\n";
            for (int y = 0; y < weightsOin.GetLength(1); y++)
            {
                for (int x = 0; x < weightsOin.GetLength(0); x++)
                {
                    returnMe += weightsOin[x, y] + ",";
                }
                returnMe = returnMe.Remove(returnMe.Length - 1);
                returnMe += "\n";
            }

            returnMe += "[WeightsOOut]\n";
            for (int y = 0; y < weightsOout.GetLength(1); y++)
            {
                for (int x = 0; x < weightsOout.GetLength(0); x++)
                {
                    returnMe += weightsOout[x, y] + ",";
                }
                returnMe = returnMe.Remove(returnMe.Length - 1);
                returnMe += "\n";
            }

            returnMe += "[BiasO]\n";
            returnMe += string.Join(",", biaso) + "\n";

            return returnMe;
        }              
    }
}
