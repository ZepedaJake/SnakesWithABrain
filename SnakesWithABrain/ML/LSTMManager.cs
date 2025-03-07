using SnakesWithABrain.Interfaces;
using SnakesWithABrain.Models;
using SnakesWithABrain.Statics;
using System;
using System.Collections.Generic;

namespace SnakesWithABrain
{
    public class LSTMManager : INeuralNetworkManager
    {
        /// <summary>
        /// How many inputs should the LSTMs be getting?
        /// </summary>
        public int Inputs { get; set; } = 1;

        /// <summary>
        /// How many outputs will the LSTM give back?
        /// </summary>
        public int Outputs { get; set; } = 1;

        public LSTMManager(int inputs, int outputs)
        {
            Inputs = inputs;
            Outputs = outputs;
        }

        private Random rng = new Random();
        public INeuralNetwork FreshNeuralNetwork()//instantiates a random LSTM
        {
            LSTMCell temp = new LSTMCell(Inputs, Outputs);
            return temp;
        }

        /// <summary>
        /// Copies weights from an LSTM cell to a new instance
        /// </summary>
        /// <param name="copyMe"></param>
        /// <returns></returns>
        public INeuralNetwork SoftCopy(INeuralNetwork copyMe)
        {
            if (copyMe.GetType() == typeof(LSTMCell))
            {
                LSTMCell c = copyMe as LSTMCell;
                LSTMCell temp = FreshNeuralNetwork() as LSTMCell;

                temp.weightsFin = temp.CopyArray(c.weightsFin);
                temp.weightsFout = temp.CopyArray(c.weightsFout);
                temp.weightsIin = temp.CopyArray(c.weightsIin);
                temp.weightsIout = temp.CopyArray(c.weightsIout);
                temp.weightsCin = temp.CopyArray(c.weightsCin);
                temp.weightsCout = temp.CopyArray(c.weightsCout);
                temp.weightsOin = temp.CopyArray(c.weightsOin);
                temp.weightsOout = temp.CopyArray(c.weightsOout);

                temp.biasf = temp.CopyArray(c.biasf);
                temp.biasi = temp.CopyArray(c.biasi);
                temp.biasc = temp.CopyArray(c.biasc);
                temp.biaso = temp.CopyArray(c.biaso);

                temp.lastCellState = temp.CopyArray(c.lastCellState);
                temp.lastOutput = temp.CopyArray(c.lastOutput);

                return temp;
            }
            else
            {
                throw new Exception($"Input neural network {copyMe.GetType().ToString()} does not match the expected type (LSTMCell) for this {this.GetType().ToString()}.");
            }            
        }

        /// <summary>
        /// Same as soft copy but this copies Guid as well
        /// </summary>
        /// <param name="copyMe"></param>
        /// <returns></returns>
        public INeuralNetwork HardCopy(INeuralNetwork copyMe)
        {
            if (copyMe.GetType() == typeof(LSTMCell))
            {
                LSTMCell c = copyMe as LSTMCell;
                LSTMCell temp = SoftCopy(c) as LSTMCell;
                temp.Guid = c.Guid;
                return temp;
            }
            else
            {
                throw new Exception($"Input neural network {copyMe.GetType().ToString()} does not match the expected type (LSTMCell) for this {this.GetType().ToString()}.");
            }
        }

        /// <summary>
        /// Breeds 2 LSTM Cells, taking weigts from both parents.
        /// </summary>
        /// <param name="parentA"></param>
        /// <param name="parentB"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public INeuralNetwork Breed(INeuralNetwork parentA, INeuralNetwork parentB)
        {
            if(parentA.GetType() == typeof(LSTMCell))
            {
                if(parentB.GetType() == typeof(LSTMCell))
                {
                    LSTMCell a = parentA as LSTMCell;
                    LSTMCell b = parentB as LSTMCell;
                    LSTMCell child = FreshNeuralNetwork() as LSTMCell;

                    for (int x = 0; x < child.outputSize; x++)
                    {
                        for (int y = 0; y < child.inputSize; y++)
                        {
                            child.weightsFin[x, y] = SelectWeight(a.weightsFin[x, y], b.weightsFin[x, y]);
                            child.weightsIin[x, y] = SelectWeight(a.weightsIin[x, y], b.weightsIin[x, y]);
                            child.weightsCin[x, y] = SelectWeight(a.weightsCin[x, y], b.weightsCin[x, y]);
                            child.weightsOin[x, y] = SelectWeight(a.weightsOin[x, y], b.weightsOin[x, y]);

                        }

                        for (int z = 0; z < child.outputSize; z++)
                        {
                            child.weightsFout[x, z] = SelectWeight(a.weightsFout[x, z], b.weightsFout[x, z]);
                            child.weightsIout[x, z] = SelectWeight(a.weightsIout[x, z], b.weightsFout[x, z]);
                            child.weightsOout[x, z] = SelectWeight(a.weightsOout[x, z], b.weightsOout[x, z]);
                            child.weightsCout[x, z] = SelectWeight(a.weightsCout[x, z], b.weightsCout[x, z]);
                        }

                        child.biasf[x] = SelectWeight(a.biasf[x], b.biasf[x]);
                        child.biasi[x] = SelectWeight(a.biasi[x], b.biasi[x]);
                        child.biasc[x] = SelectWeight(a.biasc[x], b.biasc[x]);
                        child.biaso[x] = SelectWeight(a.biaso[x], b.biaso[x]);
                    }

                    child = Mutate(child) as LSTMCell;

                    return child;
                }
                else
                {
                    throw new Exception($"ParentB neural network {parentB.GetType().ToString()} does not match the expected type (LSTMCell) for this {this.GetType().ToString()}.");
                }
            }
            else
            {
                throw new Exception($"ParentA neural network {parentA.GetType().ToString()} does not match the expected type (LSTMCell) for this {this.GetType().ToString()}.");
            }            
        }
       
        private double SelectWeight(double a, double b)
        {
            double temp = 0;

            if (rng.Next(2) == 0)
            {
                temp = a;
            }
            else
            {
                temp = b;
            }

            return temp;
        }

        /// <summary>
        /// Mutates an LSTM Cell, changing some of its weights
        /// </summary>
        /// <param name="mutateMe"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public INeuralNetwork Mutate(INeuralNetwork mutateMe)
        {
            if(mutateMe.GetType() == typeof(LSTMCell))
            {
                LSTMCell c = SoftCopy(mutateMe) as LSTMCell;//get a copy just in case this tries to stay as a reference.
                for (int x = 0; x < c.outputSize; x++)
                {
                    for (int y = 0; y < c.inputSize; y++)
                    {
                        c.weightsFin[x, y] = TryMutate(c.weightsFin[x, y]);
                        c.weightsIin[x, y] = TryMutate(c.weightsIin[x, y]);
                        c.weightsCin[x, y] = TryMutate(c.weightsCin[x, y]);
                        c.weightsOin[x, y] = TryMutate(c.weightsOin[x, y]);

                    }

                    for (int z = 0; z < c.outputSize; z++)
                    {
                        c.weightsFout[x, z] = TryMutate(c.weightsFout[x, z]);
                        c.weightsIout[x, z] = TryMutate(c.weightsIout[x, z]);
                        c.weightsOout[x, z] = TryMutate(c.weightsOout[x, z]);
                        c.weightsCout[x, z] = TryMutate(c.weightsCout[x, z]);
                    }

                    c.biasf[x] = TryMutate(c.biasf[x]);
                    c.biasi[x] = TryMutate(c.biasi[x]);
                    c.biasc[x] = TryMutate(c.biasc[x]);
                    c.biaso[x] = TryMutate(c.biaso[x]);
                }
                return c;
            }
            else
            {
                throw new Exception($"Input neural network {mutateMe.GetType().ToString()} does not match the expected type (LSTMCell) for this {this.GetType().ToString()}.");
            }

        }

       
        double TryMutate(double a)
        {
            //increase or decrease value.
            //2 here makes it so you can go above 1 and increase the value.
            if (rng.Next(101) < Globals.CurrentTrainingSession.mutateChance)
            {
                a *= (double)rng.NextDouble() * 2;
            }

            //sign swap
            if (rng.Next(101) < Globals.CurrentTrainingSession.mutateChance)
            {
                a = -a;
            }

            return a;
        }
    }
}
