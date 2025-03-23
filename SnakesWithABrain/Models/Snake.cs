using OpenTK.Platform.Windows;
using ScottPlot;
using SnakesWithABrain.Enums;
using SnakesWithABrain.Interfaces;
using SnakesWithABrain.Statics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup.Localizer;
using System.Windows.Media.Animation;

namespace SnakesWithABrain.Models
{
    public class Snake
    {
        //Fields to Save
        //NeuralNetwork
        //Guid
        //Generation
        //StartingPosX
        //StartingPosY
        //FoodLocationsSeen

        /// <summary>
        /// Neural network of this snake.
        /// </summary>
        public INeuralNetwork NeuralNetwork;

        /// <summary>
        /// Unique identifier for this snake.
        /// </summary>
        public string Guid { get; set; } = "";

        /// <summary>
        /// Which generation is this snake originally from?
        /// </summary>
        public int Generation { get; set; } = 0;
        /// <summary>
        /// Current X position.
        /// </summary>
        public int PositionX { get; set; } = 0;
        /// <summary>
        /// Current Y position.
        /// </summary>
        public int PositionY { get; set; } = 0;

        /// <summary>
        /// Starting X position.
        /// </summary>
        public int StartingPositionX { get; set; } = 0;
        /// <summary>
        /// Starting Y position.
        /// </summary>
        public int StartingPositionY { get; set; } = 0;
        /// <summary>
        /// How many food this snake has eaten.
        /// </summary>
        public int FoodEaten { get; set; } = 0;

        /// <summary>
        /// Calculated food X position. This can differ from the game board food X position when wrapping is enabled. This is also the value fed into inputs.
        /// </summary>
        private int virtualFoodX = 0;

        /// <summary>
        /// Calculated food Y position. This can differ from the game board food Y position when wrapping is enabled. This is also the value fed into inputs.
        /// </summary>
        public int virtualFoodY = 0;

        private double _distanceFromFood = 0.0;
        /// <summary>
        /// How far from food is this snake.
        /// </summary>
        public double DistanceFromFood 
        {
            get { return _distanceFromFood; }
        }

        private double _distanceFromFoodAtSpawn = 0.0f;
        /// <summary>
        /// Distance between snake and food when food first spawns in.
        /// </summary>
        public double DistanceFromFoodAtSpawn
        {
            get
            {
                return _distanceFromFoodAtSpawn;
            }
        }

        private int _movesSinceLastFood = 0;
        /// <summary>
        /// How many moves have been made since food was eaten last?
        /// </summary>
        public int MovesSinceLastFood
        {
            get
            {
                return _movesSinceLastFood;
            }
        }     

        /// <summary>
        /// List of segment locations.
        /// </summary>
        public List<Block> Segments { get; set; } = new List<Block>();

        /// <summary>
        /// Did this snake die?
        /// </summary>
        public bool IsDead { get; set; } = false;

        /// <summary>
        /// How did this snake die? Defaults to starve since the other outcomes are less desireable.
        /// </summary>
        public DeathType DeathType { get; set; } = DeathType.Starve;

        /// <summary>
        /// Timestamp of when this snake died.
        /// </summary>
        public DateTime TimeOfDeath { get; set; }

        /// <summary>
        /// List of food locations that this snake saw. Keeping track of this for saving replays.
        /// </summary>
        public List<Block> FoodLocationsSeen { get; set; } = new List<Block>();


        private double[] _inputs;
        /// <summary>
        /// Inputs to send to the INeuralNetwork.
        /// </summary>       
        public double[] Inputs
        {
            get
            {
                return _inputs;
            }
        }

        /// <summary>
        /// Game score given to snake. This is different than fitness. Starts low because a negative score can be obtained. 
        /// Big negative means small negatives will still be ordered right without counting snakes that have not played.
        /// </summary>
        public double Score = -9999999;

        /// <summary>
        /// Training score derived from game score and food eaten.
        /// </summary>
        public double Fitness 
        { 
            get
            {
                return Score / (FoodEaten + 1);
            }
        }

        /// <summary>
        /// How many moves until this snake starves. Replenished by eating food.
        /// </summary>
        public int Life { get; set; } = 0;

        public NetworkType NetworkType { get; set; }

        /// <summary>
        /// Simple Constructor. A snakes needs a brain.
        /// </summary>
        /// <param name="neuralNetwork">pp</param>
        public Snake(INeuralNetwork neuralNetwork)
        {
            NeuralNetwork = neuralNetwork;
            Guid = System.Guid.NewGuid().ToString();
            //assign same guid to this NN
            NeuralNetwork.Guid = Guid;
        }

        /// <summary>
        /// Random number generator
        /// </summary>
        private Random rng = new Random();

        /// <summary>
        /// Constructs a snake from a saved file. should also construct the brain from a saved file.
        /// </summary>
        /// <param name="loadString"></param>
        public Snake(string loadString)
        {
            string[] lines = loadString.Split('\n');
            int targetIndex = Array.IndexOf(lines, "[Guid]") + 1;//line after identifier is the info
            Guid = lines[targetIndex];

            targetIndex = Array.IndexOf(lines, "[Generation]") + 1;
            Generation = int.Parse(lines[targetIndex]);

            targetIndex = Array.IndexOf(lines, "[StartingPositionX]") + 1;
            StartingPositionX = int.Parse(lines[targetIndex]);
            targetIndex = Array.IndexOf(lines, "[StartingPositionY]") + 1;
            StartingPositionY = int.Parse(lines[targetIndex]);
            targetIndex = Array.IndexOf(lines, "[FoodLocationsSeen]") + 1;
            while (targetIndex < lines.Length && lines[targetIndex].Contains(","))
            {
                string[] pos = lines[targetIndex].Split(',');
                FoodLocationsSeen.Add(new Block(int.Parse(pos[0]), int.Parse(pos[1])));
            }

            //then load the brain
            switch (Globals.CurrentTrainingSession.networkType)
            {
                case NetworkType.LSTMCell:
                    {
                        NeuralNetwork = FileManager.LoadLSTMCell(Guid);
                        break;
                    }
            }
        }


        /// <summary>
        /// Set Properties for this snake's training session.
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        public void PrepareForTraining(int startX = -1, int startY = -1)
        {
            Life = (int)(MoreMath.Hypotenuse(Globals.CurrentTrainingSession.arrayX, Globals.CurrentTrainingSession.arrayY) * 2);//set life in relation to play area size and snake size.
            Score = 0;
            Generation = Globals.CurrentTrainingSession.generation;
            DeathType = Enums.DeathType.Starve;//default back to starve
            Segments.Clear();
            FoodEaten = 0;
            _movesSinceLastFood = 0;
            FoodLocationsSeen.Clear();
            if (startX == -1)
            {
                PositionX = rng.Next(Globals.CurrentTrainingSession.arrayX);
            }
            else
            {
                PositionX = startX;
            }

            if (startY == -1) 
            {
                PositionY = rng.Next(Globals.CurrentTrainingSession.arrayY);
            }
            else
            {
                PositionY = startY;
            }
            StartingPositionX = PositionX;
            StartingPositionY = PositionY;
            Guid = System.Guid.NewGuid().ToString();//Update GUID so that if this is a recycled snake and is saved again it does not overwrite its previous file
            NeuralNetwork.Guid = Guid;
            UpdateDistanceFromFood();
            UpdateDisatanceFromFoodAtSpawn();           
        }

        /// <summary>
        /// Does what it says. mostly called at end of move function, also should be called right after spawning a new food
        /// </summary>
        //Have an update function instead of just calculating on propery so that the math for this only has to run once per move instead of every time the distance is used.
        public void UpdateDistanceFromFood()
        {
            double distX = Math.Abs(Globals.CurrentTrainingSession.foodX - PositionX);
            virtualFoodX = Globals.CurrentTrainingSession.foodX;
            if (Globals.CurrentTrainingSession.canWrap)
            {
                if (distX > (Globals.CurrentTrainingSession.arrayX / 2))
                {
                    distX = Globals.CurrentTrainingSession.arrayX - distX;
                    if (Globals.CurrentTrainingSession.foodX > PositionX)//food is to left, but wrapping right is closer. set virtual X to X-arrayX
                    {
                        virtualFoodX -= Globals.CurrentTrainingSession.arrayX;
                    }
                    else if (Globals.CurrentTrainingSession.foodX < PositionX)//food is to left, but wrapping right is closer
                    {
                        virtualFoodX += Globals.CurrentTrainingSession.arrayX;
                    }
                }

            }

            double distY = Math.Abs(Globals.CurrentTrainingSession.foodY - PositionY);
            virtualFoodY = Globals.CurrentTrainingSession.foodY;
            if (Globals.CurrentTrainingSession.canWrap)
            {
                if (distY > (Globals.CurrentTrainingSession.arrayY / 2))
                {
                    distY = Globals.CurrentTrainingSession.arrayY - distY;
                    if (Globals.CurrentTrainingSession.foodY > PositionY)//food is to bottom, but wrapping top is closer. set virtual Y to Y-arrayY
                    {
                        virtualFoodY -= Globals.CurrentTrainingSession.arrayY;
                    }
                    else if (Globals.CurrentTrainingSession.foodY < PositionY)//food is to  top, but wrapping bottom is closer
                    {
                        virtualFoodY += Globals.CurrentTrainingSession.arrayY;
                    }
                }
            }

            double dist = Math.Sqrt(Math.Pow(distX, 2) + Math.Pow(distY, 2));
            _distanceFromFood = dist;
        }

        /// <summary>
        /// Does what it says...
        /// </summary>
        public void UpdateDisatanceFromFoodAtSpawn()
        {
            _distanceFromFoodAtSpawn = _distanceFromFood;//we alread ahve the distance from food, we just want to save how far it was when it spawned in for scoring purposes.
        }

        /// <summary>
        /// Moves snake, updates segments, then perfoms collision checks for self, walls, and food
        /// </summary>
        public void Move()
        {
            _movesSinceLastFood++;//update private var

            Segments.Add(new Block(PositionX, PositionY));

            if (Segments.Count > (FoodEaten + Globals.CurrentTrainingSession.startingLength) || (Globals.CurrentTrainingSession.maxSegmentLength > -1 && Segments.Count > Globals.CurrentTrainingSession.maxSegmentLength))
            {
                Segments.RemoveAt(0);
            }

            MoveDirection dir = SelectMoveDirection();
            //grid is top to bottom, left to right
            // 0,0 | 0,1 | 0,2
            // 1,0 | 1,1 | 1,2
            // 2,0 | 2,1 | 2,2
            switch (dir)
            {
                case MoveDirection.UpLeft:
                    {
                        PositionX -= 1;
                        PositionY -= 1;
                        break;
                    }
                case MoveDirection.Up:
                    {
                        PositionY -= 1;
                        break;
                    }
                case MoveDirection.UpRight:
                    {
                        PositionX += 1;
                        PositionY -= 1;
                        break;
                    }
                case MoveDirection.Left:
                    {
                        PositionX -= 1;
                        break;
                    }
                case MoveDirection.Right:
                    {
                        PositionX += 1;
                        break;
                    }
                case MoveDirection.DownLeft:
                    {
                        PositionX -= 1;
                        PositionY += 1;
                        break;
                    }
                case MoveDirection.Down:
                    {
                        PositionY += 1;
                        break;
                    }
                case MoveDirection.DownRight:
                    {
                        PositionX += 1;
                        PositionY += 1;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            UpdateDistanceFromFood();

            //segments have been added and position updated.
            //Now do collision checks.
            Life--;//subtrace 1 before collision check. 1  move = 1 life
            CollisionCheck();
            if(Life == 0)
            {
                TimeOfDeath = DateTime.Now;
            }
        }

        void CollisionCheck()
        {
            //check for crash on X axis
            if (PositionX < 0 || PositionX >= Globals.CurrentTrainingSession.arrayX)
            {
                //hit wall, die
                if (!Globals.CurrentTrainingSession.canWrap)
                {                   
                    DeathType = Enums.DeathType.Crash;
                    IsDead = true;
                }
                else if (PositionX < 0)//wrap to right
                {
                    PositionX = Globals.CurrentTrainingSession.arrayX - 1;
                }
                else if (PositionX >= Globals.CurrentTrainingSession.arrayX)//wrap to left
                {
                    PositionX = 0;
                }
            }

            //check for crash on Y axis
            if (!IsDead && (PositionY < 0 || PositionY >= Globals.CurrentTrainingSession.arrayY))
            {
                if (!Globals.CurrentTrainingSession.canWrap)
                {
                    //hit wall, die
                    DeathType = Enums.DeathType.Crash;
                    IsDead = true;
                }
                else if (PositionY < 0)//wrap to bottom
                {
                    PositionY = Globals.CurrentTrainingSession.arrayY - 1;
                }
                else if (PositionY >= Globals.CurrentTrainingSession.arrayY)//wrap to top
                {
                    PositionY = 0;
                }
            }

            //check if ate self.
            if (!IsDead)
            {
                Block hitTest = Segments.FirstOrDefault(x => x.X == PositionX && x.Y == PositionY);
                if (hitTest != null)
                {
                    //ate self. die.
                    DeathType = Enums.DeathType.EatSelf;
                    IsDead = true;
                }
            }

            if (IsDead)
            {
                Score -= (float)Math.Pow(MoreMath.Hypotenuse(Globals.CurrentTrainingSession.arrayX, Globals.CurrentTrainingSession.arrayY) * (double)Life, (double)Globals.CurrentTrainingSession.crashScaling);
                Life = 0;
                TimeOfDeath = DateTime.Now;
                return;
            }

            //if not dead, check if collided with food
            if (PositionX == virtualFoodX && PositionY == virtualFoodY)
            {
                //touching food, eat it and add score and some life.
                FoodEaten++;
                Globals.CurrentTrainingSession.newFoodNeeded = true;//also report back to the training session that it needs to make more food

                //Add score.
                double scoreAdd = 0;
                //scoreAdd = MoreMath.Hypotenuse(Globals.CurrentTrainingSession.arrayX, Globals.CurrentTrainingSession.arrayY) * (DistanceFromFoodAtSpawn / (double)MovesSinceLastFood) * (Math.Pow(FoodEaten+1,Globals.CurrentTrainingSession.foodEatScaling));
                //scoreAdd = Math.Pow(scoreAdd, Globals.CurrentTrainingSession.foodEatScaling);
                //scoreAdd += 100 * MoreMath.Hypotenuse(Globals.CurrentTrainingSession.arrayX, Globals.CurrentTrainingSession.arrayY) * (DistanceFromFoodAtSpawn / (double)MovesSinceLastFood);

                scoreAdd += 50 * MoreMath.Hypotenuse(Globals.CurrentTrainingSession.arrayX, Globals.CurrentTrainingSession.arrayY) * FoodEaten * Globals.CurrentTrainingSession.foodEatScaling;
                Score += scoreAdd;
                Life += (int)MoreMath.Hypotenuse(Globals.CurrentTrainingSession.arrayX, Globals.CurrentTrainingSession.arrayY) + Segments.Count();
                _movesSinceLastFood = 0;//reset moves since last food
            }
            else
            {
                //lower score based on how far from food
                Score -= DistanceFromFood;
            }
        }

        /// <summary>
        /// Gets the Move Direction this snake wants to go based on the inputs
        /// </summary>
        /// <returns></returns>
        private MoveDirection SelectMoveDirection()
        {
            UpdateInputs();
            double[] selection = NeuralNetwork.Compute(Inputs);
            double highestVal = -10000;//some dumb low number that should easily be overwritten
            int maxIndex = 0;
            for (int x = 0; x < selection.Length; x++)
            {
                if (selection[x] > highestVal)
                {
                    maxIndex = x;
                    highestVal = selection[x];
                }
            }

            return (MoveDirection)maxIndex;
        }
        private void UpdateInputs()
        {
            double[] inputs = new double[1];
            Block hitTest = null;
            int testPosX = 0;
            int testPosY = 0;
            switch (Globals.CurrentTrainingSession.inputType)
            {
                case 1:
                    {
                        inputs = new double[14];
                        #region Surrounding area, food location, food distance, snake location, segment count
                        //top left
                        testPosX = PositionX - 1;
                        testPosY = PositionY - 1;
                        hitTest = Segments.FirstOrDefault(x => x.X == testPosX && x.Y == testPosY);
                        if ((!Globals.CurrentTrainingSession.canWrap && (PositionX == 0 && PositionY == 0)) || (Segments.Count > 0 && hitTest != null))
                        {
                            inputs[0] = 1;
                        }

                        //top
                        testPosX = PositionX;
                        testPosY = PositionY - 1;
                        hitTest = Segments.FirstOrDefault(x => x.X == testPosX && x.Y == testPosY);
                        if ((!Globals.CurrentTrainingSession.canWrap && PositionY == 0) || (Segments.Count > 0 && hitTest != null))
                        {
                            inputs[1] = 1;
                        }

                        //top right
                        testPosX = PositionX + 1;
                        testPosY = PositionY - 1;
                        hitTest = Segments.FirstOrDefault(x => x.X == testPosX && x.Y == testPosY);
                        if ((!Globals.CurrentTrainingSession.canWrap && (PositionX == Globals.CurrentTrainingSession.arrayX - 1 && PositionY == 0)) || (Segments.Count > 0 && hitTest != null))
                        {
                            inputs[2] = 1;
                        }

                        //left
                        testPosX = PositionX - 1;
                        testPosY = PositionY;
                        hitTest = Segments.FirstOrDefault(x => x.X == testPosX && x.Y == testPosY);
                        if ((!Globals.CurrentTrainingSession.canWrap && PositionX == 0) || (Segments.Count > 0 && hitTest != null))
                        {
                            inputs[3] = 1;
                        }

                        //right
                        testPosX = PositionX + 1;
                        testPosY = PositionY;
                        hitTest = Segments.FirstOrDefault(x => x.X == testPosX && x.Y == testPosY);
                        if ((!Globals.CurrentTrainingSession.canWrap && PositionX == Globals.CurrentTrainingSession.arrayX - 1) || (Segments.Count > 0 && hitTest != null))
                        {
                            inputs[4] = 1;
                        }

                        //bottom left
                        testPosX = PositionX - 1;
                        testPosY = PositionY + 1;
                        hitTest = Segments.FirstOrDefault(x => x.X == testPosX && x.Y == testPosY);
                        if ((!Globals.CurrentTrainingSession.canWrap && (PositionX == 0 && PositionY == Globals.CurrentTrainingSession.arrayY - 1)) || (Segments.Count > 0 && hitTest != null))
                        {
                            inputs[5] = 1;
                        }

                        //bottom
                        testPosX = PositionX;
                        testPosY = PositionY + 1;
                        hitTest = Segments.FirstOrDefault(x => x.X == testPosX && x.Y == testPosY);
                        if ((!Globals.CurrentTrainingSession.canWrap && PositionY == Globals.CurrentTrainingSession.arrayY - 1) || (Segments.Count > 0 && hitTest != null))
                        {
                            inputs[6] = 1;
                        }

                        //bottom right
                        testPosX = PositionX + 1;
                        testPosY = PositionY + 1;
                        hitTest = Segments.FirstOrDefault(x => x.X == testPosX && x.Y == testPosY);
                        if ((!Globals.CurrentTrainingSession.canWrap && (PositionX == Globals.CurrentTrainingSession.arrayX - 1 && PositionY == Globals.CurrentTrainingSession.arrayY - 1)) || (Segments.Count > 0 && hitTest != null))
                        {
                            inputs[7] = 1;
                        }

                        inputs[8] = PositionX;
                        inputs[9] = PositionY;
                        inputs[10] = virtualFoodX;
                        inputs[11] = virtualFoodY;
                        inputs[12] = Segments.Count;
                        inputs[13] = DistanceFromFood;
                        #endregion
                        break;
                    }
                case 2:
                    {
                        inputs = new Double[28];
                        #region Surrounding Coords and are they blocked            
                        //top left
                        inputs[0] = PositionX - 1;
                        inputs[1] = PositionY - 1;
                        hitTest = Segments.FirstOrDefault(x => x.X == inputs[0] && x.Y == inputs[1]);
                        if ((!Globals.CurrentTrainingSession.canWrap && (PositionX == 0 && PositionY == 0)) || (Segments.Count > 0 && hitTest != null))
                        {
                            inputs[2] = 1;
                        }

                        //top
                        inputs[3] = PositionX;
                        inputs[4] = PositionY - 1;
                        hitTest = Segments.FirstOrDefault(x => x.X == inputs[3] && x.Y == inputs[4]);
                        if ((!Globals.CurrentTrainingSession.canWrap && PositionY == 0) || (Segments.Count > 0 && hitTest != null))
                        {
                            inputs[5] = 1;
                        }

                        //top right
                        inputs[6] = PositionX + 1;
                        inputs[7] = PositionY - 1;
                        hitTest = Segments.FirstOrDefault(x => x.X == inputs[6] && x.Y == inputs[7]);
                        if ((!Globals.CurrentTrainingSession.canWrap && (PositionX == Globals.CurrentTrainingSession.arrayX - 1 && PositionY == 0)) || (Segments.Count > 0 && hitTest != null))
                        {
                            inputs[8] = 1;
                        }

                        //left
                        inputs[9] = PositionX - 1;
                        inputs[10] = PositionY;
                        hitTest = Segments.FirstOrDefault(x => x.X == inputs[9] && x.Y == inputs[10]);
                        if ((!Globals.CurrentTrainingSession.canWrap && PositionX == 0) || (Segments.Count > 0 && hitTest != null))
                        {
                            inputs[11] = 1;
                        }

                        //right
                        inputs[12] = PositionX + 1;
                        inputs[13] = PositionY;
                        hitTest = Segments.FirstOrDefault(x => x.X == inputs[12] && x.Y == inputs[13]);
                        if ((!Globals.CurrentTrainingSession.canWrap && PositionX == Globals.CurrentTrainingSession.arrayX - 1) || (Segments.Count > 0 && hitTest != null))
                        {
                            inputs[14] = 1;
                        }

                        //bottom left
                        inputs[15] = PositionX - 1;
                        inputs[16] = PositionY + 1;
                        hitTest = Segments.FirstOrDefault(x => x.X == inputs[15] && x.Y == inputs[16]);
                        if ((!Globals.CurrentTrainingSession.canWrap && (PositionX == 0 && PositionY == Globals.CurrentTrainingSession.arrayY - 1)) || (Segments.Count > 0 && hitTest != null))
                        {
                            inputs[17] = 1;
                        }

                        //bottom
                        inputs[18] = PositionX;
                        inputs[19] = PositionY + 1;
                        hitTest = Segments.FirstOrDefault(x => x.X == inputs[18] && x.Y == inputs[19]);
                        if ((!Globals.CurrentTrainingSession.canWrap && PositionY == Globals.CurrentTrainingSession.arrayY - 1) || (Segments.Count > 0 && hitTest != null))
                        {
                            inputs[20] = 1;
                        }

                        //bottom right
                        inputs[21] = PositionX + 1;
                        inputs[22] = PositionY + 1;
                        hitTest = Segments.FirstOrDefault(x => x.X == inputs[21] && x.Y == inputs[22]);
                        if ((!Globals.CurrentTrainingSession.canWrap && (PositionX == Globals.CurrentTrainingSession.arrayX - 1 && PositionY == Globals.CurrentTrainingSession.arrayY - 1)) || (Segments.Count > 0 && hitTest != null))
                        {
                            inputs[23] = 1;
                        }

                        inputs[24] = PositionX;
                        inputs[25] = PositionY;
                        inputs[26] = virtualFoodX;
                        inputs[27] = virtualFoodY;
                        #endregion
                        break;
                    }
                case 3://my position, food position, segment count, grid status
                    {
                        inputs = new double[6 + (Globals.CurrentTrainingSession.arrayX * Globals.CurrentTrainingSession.arrayY)];//large array, but tells the NN the status of the whole grid.
                        inputs[0] = PositionX;
                        inputs[1] = PositionY;
                        inputs[2] = DistanceFromFood;
                        inputs[3] = virtualFoodX;
                        inputs[4] = virtualFoodY;
                        inputs[5] = Segments.Count;
                        for (int x = Globals.CurrentTrainingSession.arrayX; x < Globals.CurrentTrainingSession.arrayX; x++)
                        {
                            for (int y = Globals.CurrentTrainingSession.arrayY; y < Globals.CurrentTrainingSession.arrayY; y++)
                            {
                                int index = 5 + (x + 1) * y;
                                if (Segments.FirstOrDefault(a => a.X == x && a.Y == y) != null)
                                {
                                    inputs[index] = 1;
                                }
                            }

                        }
                        break;
                    }
                default:
                    {

                        break;
                    }
            }
            _inputs = inputs;
        }

        /// <summary>
        /// Creates file contents for use from teh file manager.
        /// This is really more like "ToString()"
        /// </summary>
        /// <returns></returns>
        public string Save()
        {
            string returnMe = "";
            returnMe += $"[Guid]\n{Guid}\n";
            returnMe += $"[Generation]\n{Generation}\n";
            returnMe += $"[StartingPositionX]\n{StartingPositionX}\n";
            returnMe += $"[StartingPositionY]\n{StartingPositionY}\n";
            returnMe += "[FoodLocationsSeen]";
            foreach (Block b in FoodLocationsSeen) 
            {
                returnMe += $"{b.X},{b.Y}";
            }

            return returnMe;
        }
    }
}
