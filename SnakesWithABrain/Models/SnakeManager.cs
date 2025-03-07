using SnakesWithABrain.Interfaces;
using SnakesWithABrain.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SnakesWithABrain.Models
{
    /// <summary>
    /// Manages the snakes that are competing.
    /// </summary>
    public class SnakeManager
    {
        /// <summary>
        /// The snake that is currently training.
        /// </summary>
        public Snake currentSnake;

        /// <summary>
        /// Snakes that will compete this generation.
        /// </summary>
        public List<Snake> thisGenerationSnakes = new List<Snake>();
        /// <summary>
        /// Best snakes across generations. Like a hall of fame, except sometimes a snake will get erased from history if "Random Death Chance" is configured for the training session.
        /// </summary>
        public List<Snake> bestSnakes = new List<Snake>();

        private Random rng = new Random();

        /// <summary>
        /// Neural Network manager. Manages copying networks, as well as breeding/mutating.
        /// </summary>
        private INeuralNetworkManager neuralNetworkManager;

        public SnakeManager(INeuralNetworkManager nnm)
        {
            //just creating a LSTM Manager for testing.
            neuralNetworkManager = nnm;
        }

        /// <summary>
        /// List of best average fitness per generation over X generations
        /// </summary>
        public List<double> bestAvgFitnessesPerGeneration = new List<double>();

        /// <summary>
        /// List of most food eaten per generation over X generations
        /// </summary>
        public List<double> mostFoodEatenPerGeneration = new List<double>();
        public void InitSnakes()
        {
            thisGenerationSnakes.Clear();
            for (int i = 0; i < Globals.CurrentTrainingSession.genSize; i++) //init blank nets for the start of the game
            {
                //create new snake with a brain (neural network) and add it to list
                thisGenerationSnakes.Add(new Snake(neuralNetworkManager.FreshNeuralNetwork()));
            }

            for (int i = 0; i < Globals.CurrentTrainingSession.keepCount; i++)
            {
                //fill missing spots in the best snakes list with placeholder snakes.
                bestSnakes.Add(new Snake(neuralNetworkManager.FreshNeuralNetwork()));
            }
        }

        public void NextSnake()
        {
            //Check if end of generation has been reached
            if (Globals.CurrentTrainingSession.snakeIndex >= Globals.CurrentTrainingSession.genSize)
            {
                GenerationEnd();
                GenerationStart();
            }

            //Select next snake in list, set life based on board size, reset score. set snake's generation, reset snake's death type.
           


            currentSnake = thisGenerationSnakes[Globals.CurrentTrainingSession.snakeIndex];            
            currentSnake.PrepareForTraining();
            Globals.CurrentTrainingSession.snakeIndex++;//increment after selecting snake so next time the next snake is selected
        }
        public void GenerationStart()
        {
            //reset current snake list. and increment generation because we are preparing the next generation.
            thisGenerationSnakes.Clear();
            Globals.CurrentTrainingSession.generation++;
            Globals.CurrentTrainingSession.snakeIndex = 0;

            //let the best recorded snakes compete again
            for (int a = 0; a < Globals.CurrentTrainingSession.keepCount; a++) //recompete
            {
                if(a < bestSnakes.Count)
                {
                    thisGenerationSnakes.Add(CopySnake(bestSnakes[a]));
                }
                else
                {
                    break;
                }
            }

            //breed some of the best snakes and let thos children compete
            for (int b = 0; b < Globals.CurrentTrainingSession.breedCount; b++) 
            {
                int parentA = rng.Next(bestSnakes.Count);
                int parentB = -1;
                //roll parent B until it is unique and non-negative
                while(parentB < 0  || parentB == parentA)
                {
                    parentB = rng.Next(Globals.CurrentTrainingSession.breedCount);
                }
                INeuralNetwork bredBrain = neuralNetworkManager.Breed(bestSnakes[parentA].NeuralNetwork, bestSnakes[parentB].NeuralNetwork);
                thisGenerationSnakes.Add(new Snake(bredBrain));
            }

            //make mutated versions of some of the best and let those abominations compete
            for(int c = 0; c < Globals.CurrentTrainingSession.mutateCount; c++)
            {
                int selected = rng.Next(bestSnakes.Count);
                INeuralNetwork mutatedBrain = neuralNetworkManager.Mutate(bestSnakes[selected].NeuralNetwork);
                thisGenerationSnakes.Add(new Snake(mutatedBrain));
            }

            //fill remainder of generation with new snakes
            int spaceToFill = Globals.CurrentTrainingSession.genSize - thisGenerationSnakes.Count;
            if(spaceToFill > 0)
            {
                for (int d = 0; d < spaceToFill; d++) 
                {
                    thisGenerationSnakes.Add(new Snake(neuralNetworkManager.FreshNeuralNetwork()));
                }
            }            
        }

        public void GenerationEnd()
        {
            Globals.CurrentTrainingSession.snakeIndex = 0;
            double bestFitnessThisGen = 0;
            Snake bestSnakeThisGeneration = thisGenerationSnakes.OrderByDescending(x => x.Fitness).First();
            Snake hungriestSnakeThisGeneration = thisGenerationSnakes.OrderByDescending(x => x.FoodEaten).First();
            if (Globals.CurrentTrainingSession.generation > 1 && Globals.CurrentTrainingSession.generation % 1000 == 0)//every 1000 generations save the best snake
            {
                //SaveReplay(bestSnakeThisGeneration);
            }

            //random death            
            int doDie = rng.Next(1, 101);
            if (doDie <= Globals.CurrentTrainingSession.randomDeathChance)
            {
                int randindex = rng.Next(0, bestSnakes.Count());
                bestSnakes.RemoveAt(randindex);//remove snake from list
            }
            bestSnakes.Add(bestSnakeThisGeneration);//now add the best for this generation to the list finally.
            bestSnakes = bestSnakes.OrderByDescending(x => x.Fitness).ToList();

            if (bestSnakes.Count > Globals.CurrentTrainingSession.keepCount)
            {
                bestSnakes.RemoveAt(bestSnakes.Count() - 1);//Remove Last best. if one was overwritten, this one will be removed.
            }

            //checking first here, because it will only trigger if the new best snake hsa the overall best fitness anyways.
            if (bestSnakes.First().Fitness > Globals.CurrentTrainingSession.bestFitnessEver)
            {
                Globals.CurrentTrainingSession.bestFitnessEver = bestSnakes.First().Fitness;
                //SaveReplay(bestSnakes.First());
            }

            bestAvgFitnessesPerGeneration.Add(GetBestAverageFitness());
            if (bestAvgFitnessesPerGeneration.Count > 200)
            {
                //if over limit, remove oldest (first) value
                bestAvgFitnessesPerGeneration.RemoveAt(0);
            }

            mostFoodEatenPerGeneration.Add(hungriestSnakeThisGeneration.FoodEaten);
            //Checking last here,becaus food and fitness do not correlate 1:1, so we want to check the newest addition to the list.
            if (hungriestSnakeThisGeneration.FoodEaten > Globals.CurrentTrainingSession.mostFoodEver)
            {
                Globals.CurrentTrainingSession.mostFoodEver = hungriestSnakeThisGeneration.FoodEaten;
                //make sure this best snake didnt already get a replay saved. no need to save twice
                if(bestSnakeThisGeneration.Guid != hungriestSnakeThisGeneration.Guid)
                {
                    //SaveReplay(bestSnakeThisGeneration);
                }
            }

            if (mostFoodEatenPerGeneration.Count > 200)
            {
                mostFoodEatenPerGeneration.RemoveAt(0);
            }            
        }

        /// <summary>
        /// Creates a deep copy of a snake.
        /// </summary>
        /// <param name="copyMe"></param>
        /// <returns></returns>
        private Snake CopySnake(Snake copyMe)
        {
            INeuralNetwork copiedBrain = neuralNetworkManager.HardCopy(copyMe.NeuralNetwork);
            Snake newCopy = new Snake(copiedBrain);
            newCopy.Score = copyMe.Score;
            newCopy.DeathType = copyMe.DeathType;
            newCopy.TimeOfDeath = copyMe.TimeOfDeath;
            newCopy.FoodEaten = copyMe.FoodEaten;
            newCopy.StartingPositionX = copyMe.StartingPositionX;
            newCopy.StartingPositionY = copyMe.StartingPositionY;
            newCopy.Generation = copyMe.Generation;
            newCopy.FoodLocationsSeen = copyMe.FoodLocationsSeen.ToList();//doing ToList to make sure i get a deep copy
            return newCopy;
        }

        /// <summary>
        /// Gets average fitness from the list of best snakes
        /// </summary>s
        /// <returns></returns>
        private double GetBestAverageFitness()
        {
            double a = 0.0f;
            for (int i = 0; i < bestSnakes.Count; i++)
            {
                a += bestSnakes[i].Fitness;
            }
            
            return (a / bestSnakes.Count);
        }
    }
}
