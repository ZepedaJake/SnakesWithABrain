using SnakesWithABrain.Enums;
using SnakesWithABrain.Models;
using SnakesWithABrain.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SnakesWithABrain
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    /// IDEAS
    /// Log Death Type as Enum (Crash, Starve, Eat Self) DONE
    /// Create Replays (Remember where each food spawned and the starting point of the snake)
    /// -TrainingSessions\Replays\ Inside has a file {generation}{snakeindex}{DateTime("yyyyDDMMHHmmssxxx").snkrp
    /// -replay contains the network or cell for the snake, where it started, and the order of food locations
    /// Additional settings
    /// -Autosave every X generations
    /// Parrallelize training
    /// -might be hard...
    /// -would require turning the live draw into a sort of replay instead
    /// Add labels for Training session and Input types (and descriptions)
    /// Maybe move new training session setup to its own window... then report back the object
    /// Tabs on right side for training data, and session info
    /// Maybe save FULL graph history. would require saving the data every X generations. x being less than or equal to the max history of graph currently (200)
    /// Maybe create an interface for MachineLearningManager with the Breed, Mutate, Fresh, Copy functions
    /// Also would need a MachineLearning interface or derived type for different types of networks (LSTM, Basic NN, Dropout NN)


    ///FIGURE OUT WHAT NEEDS HARD vs SOFT COPY!!
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Speaks for itself. 
        /// </summary>
        Random rng = new Random();
        /// <summary>
        /// Game/Training timer. Each tick is a step for training/play
        /// </summary>
        DispatcherTimer timer = new DispatcherTimer();
        /// <summary>
        /// Just a short countdown for redrawing the squares in the play area. Better to wait for a fraction of a second for resizing to stop than constantly resize during a drag.
        /// </summary>
        DispatcherTimer resizeTimer = new DispatcherTimer();

        //Positioning data
        /// <summary>
        /// Array of all rectanges in the play area. Stored to easily update. naming each one as x_y and referencing the main window may potentially be faster than storing the entire grid
        /// </summary>
        Rectangle[,] blockArray;//array of all cells in play area               

        //Instance Options
        bool draw = false;
        bool updateUi = false;
        bool paused = false;
        bool stopped = false;
        bool trainingReady = false;
        
        SnakeManager snakeManager;//Starts null. Setting to new initialized lists and the snakes in them.                                                          

        public MainWindow()
        {
            InitializeComponent();
            timer.Tick += TimerTick;
            timer.Interval = TimeSpan.FromSeconds(sldSimSpeed.Value);
            resizeTimer.Interval = TimeSpan.FromMilliseconds(250);//quarter second timer
            resizeTimer.Tick += ResizeTimerTick;

            cmbTrainings.Items.Add("New Training Session");
            foreach (string s in FileManager.GetTrainingSessions())
            {
                cmbTrainings.Items.Add(s);
            }

            cmbTrainings.SelectedIndex = 0;

            gridTrainingSession.Visibility = Visibility.Visible;
            gridGameData.Visibility = Visibility.Hidden;
            gridCharts.Visibility = Visibility.Hidden;
        }

        #region Grid Functions
        /// <summary>
        /// Prepares size for squares in play area then adds them to the grid on the main window.
        /// </summary>
        void SetUpGrid()
        {
            double gridWidth = gridPlayArea.ActualWidth;
            double gridHeight = gridPlayArea.ActualHeight;

            double squareSize = 1;
            if ((gridWidth / Globals.CurrentTrainingSession.arrayX) < (gridHeight / Globals.CurrentTrainingSession.arrayY))
            {
                //tall area, clamp to width
                squareSize = gridWidth / Globals.CurrentTrainingSession.arrayX;
            }
            else
            {
                squareSize = gridHeight / Globals.CurrentTrainingSession.arrayY;
            }

            blockArray = new Rectangle[Globals.CurrentTrainingSession.arrayX, Globals.CurrentTrainingSession.arrayY];

            for (int i = 0; i < Globals.CurrentTrainingSession.arrayX; i++)
            {
                for (int j = 0; j < Globals.CurrentTrainingSession.arrayY; j++)
                {
                    Rectangle rect = new Rectangle();

                    rect.Width = squareSize;
                    rect.Height = squareSize;
                    rect.Margin = new Thickness(i * squareSize, j * squareSize, 0, 0);
                    rect.Fill = new SolidColorBrush(Colors.White);
                    rect.StrokeThickness = 1;
                    rect.Stroke = new SolidColorBrush(Colors.Black);
                    rect.HorizontalAlignment = HorizontalAlignment.Left;
                    rect.VerticalAlignment = VerticalAlignment.Top;
                    blockArray[i, j] = rect;
                    gridMain.Children.Add(rect);
                }
            }
        }

        /// <summary>
        /// Resets all squares fill to White. Remembering what was changed and only restoring those may be faster, but this is simpler.
        /// </summary>
        void ResetGrid()
        {
            if (draw)
            {
                for (int i = 0; i < Globals.CurrentTrainingSession.arrayX; i++)
                {
                    for (int j = 0; j < Globals.CurrentTrainingSession.arrayY; j++)
                    {
                        blockArray[i, j].Fill = new SolidColorBrush(Colors.White);
                    }
                }
            }
        }

        /// <summary>
        /// Resizes squares in grid to fit play area
        /// </summary>
        void ResizeGrid()
        {
            double gridWidth = gridPlayArea.ActualWidth;
            double gridHeight = gridPlayArea.ActualHeight;

            double squareSize = 1;
            if ((gridWidth / Globals.CurrentTrainingSession.arrayX) < (gridHeight / Globals.CurrentTrainingSession.arrayY))
            {
                //tall area, clamp to width
                squareSize = gridWidth / Globals.CurrentTrainingSession.arrayX;
            }
            else
            {
                squareSize = gridHeight / Globals.CurrentTrainingSession.arrayY;
            }

            for (int x = 0; x < Globals.CurrentTrainingSession.arrayX; x++)
            {
                for (int y = 0; y < Globals.CurrentTrainingSession.arrayY; y++)
                {
                    blockArray[x, y].Height = squareSize;
                    blockArray[x, y].Width = squareSize;
                    blockArray[x, y].Margin = new Thickness(x * squareSize, y * squareSize, 0, 0);
                }
            }
        }
        #endregion


        void Startup()
        {
            //set food somewhere
            //select a random starting point
            //initiate LSTM
            snakeManager = new SnakeManager(new LSTMManager(Globals.CurrentTrainingSession.inputCount,8));
            snakeManager.InitSnakes();
            snakeManager.NextSnake();
            NewFood();
        }
        /// <summary>
        /// Updates UI Text and charts
        /// </summary>
        void UpdateUi()
        {
            lblLife.Content = $"Life: {snakeManager.currentSnake.Life} | Distance: {snakeManager.currentSnake.DistanceFromFood.ToString("0.000")}";
            lblCurrentFitness.Content = $"Fitness: {snakeManager.currentSnake.Fitness.ToString("0.000")} | Score: {snakeManager.currentSnake.Score.ToString("0.000")}";
            lblGenInfo.Content = $"Generation: {Globals.CurrentTrainingSession.generation} | Snake Index: {Globals.CurrentTrainingSession.snakeIndex}";

            string keepers = "Best: ";
            for (int e = 0; e < snakeManager.bestSnakes.Count; e++)
            {
                Snake temp = snakeManager.bestSnakes[e];
                keepers += $"\n{temp.Fitness.ToString("0.000")} ({temp.Score.ToString("0.000")} | {temp.FoodEaten}) at {temp.TimeOfDeath}";
            }
            lblBest.Content = keepers;

            if (snakeManager.currentSnake.DeathType != Enums.DeathType.Starve)
            {
                lblCurrentFitness.Background = new SolidColorBrush(Colors.Red);
            }
            else
            {
                lblCurrentFitness.Background = new SolidColorBrush(Colors.White);
            }

            lblBestEver.Content = $"Best Fitness Ever: {Globals.CurrentTrainingSession.bestFitnessEver}\tMost Food Ever: {Globals.CurrentTrainingSession.mostFoodEver}";

            //Charts
            if (snakeManager.bestAvgFitnessesPerGeneration.Count > 0)
            {
                double chartYMin = snakeManager.bestAvgFitnessesPerGeneration.Min();
                double chartYMax = snakeManager.bestAvgFitnessesPerGeneration.Max();

                //sequential order of last X generations.
                double[] nums = new double[200];
                if (snakeManager.bestAvgFitnessesPerGeneration.Count < nums.Length)
                {
                    nums = new double[snakeManager.bestAvgFitnessesPerGeneration.Count];
                }

                for (int i = nums.Length - 1; i >= 0; i--)
                {
                    nums[nums.Length - 1 - i] = Globals.CurrentTrainingSession.generation - 1 - i;
                }
                UpdateChart(plotAvgFitness, nums, snakeManager.bestAvgFitnessesPerGeneration.ToArray(), chartYMin, chartYMax);
            }

            if (snakeManager.mostFoodEatenPerGeneration.Count > 0)
            {
                double chartYMin = 0;
                double chartYMax = snakeManager.mostFoodEatenPerGeneration.Max() + 1;

                double[] nums = new double[200];
                if (snakeManager.mostFoodEatenPerGeneration.Count < nums.Length)
                {
                    nums = new double[snakeManager.mostFoodEatenPerGeneration.Count];
                }

                for (int i = nums.Length - 1; i >= 0; i--)
                {
                    nums[nums.Length - 1 - i] = Globals.CurrentTrainingSession.generation - 1 - i;
                }

                UpdateChart(plotFoodEaten, nums, snakeManager.mostFoodEatenPerGeneration.ToArray(), chartYMin, chartYMax);
            }
        }

        /// <summary>
        /// Updates squares representing the snake and food and updates the UI on the right panel.
        /// </summary>
        void DrawStuff()
        {
            ResetGrid();
            if(snakeManager.currentSnake.Life > 0)
            {
                blockArray[snakeManager.currentSnake.PositionX, snakeManager.currentSnake.PositionY].Fill = new SolidColorBrush(Colors.Blue);
            }
            
            foreach (Block segment in snakeManager.currentSnake.Segments)
            {
                if(snakeManager.currentSnake.Life > 0)
                {
                    blockArray[segment.X, segment.Y].Fill = new SolidColorBrush(Colors.LightBlue);
                }
                else
                {
                    blockArray[segment.X, segment.Y].Fill = new SolidColorBrush(Colors.Pink);
                }
                
            }

            blockArray[Globals.CurrentTrainingSession.foodX, Globals.CurrentTrainingSession.foodY].Fill = new SolidColorBrush(Colors.Green); //change block to green, this is 'food'                    
        }
        void NewFood()
        {
            Block hitTest = null;
            int foodX = 0;
            int foodY = 0;
            do//loop through random spots until you find one that is not the LSTMs current position
            {
                foodX = rng.Next(0, Globals.CurrentTrainingSession.arrayX);
                foodY = rng.Next(0, Globals.CurrentTrainingSession.arrayY);
                hitTest = snakeManager.currentSnake.Segments.FirstOrDefault(x => x.X == foodX && x.Y == foodY);
            }
            while ((foodX == snakeManager.currentSnake.PositionX && foodY == snakeManager.currentSnake.PositionY) || (hitTest != null));
            Globals.CurrentTrainingSession.foodX = foodX;
            Globals.CurrentTrainingSession.foodY = foodY;

            snakeManager.currentSnake.FoodLocationsSeen.Add(new Block(foodX, foodY));
            snakeManager.currentSnake.UpdateDistanceFromFood();//udpate snake's distance from food here because it only does it by itself after moving
            snakeManager.currentSnake.UpdateDisatanceFromFoodAtSpawn();//also update the distance from food at food's spawn
            Globals.CurrentTrainingSession.newFoodNeeded = false;
        }

        void TimerTick(object sender, EventArgs e)
        {
            if (paused)
            {
                return;
            }

            if (stopped && Globals.CurrentTrainingSession.snakeIndex == Globals.CurrentTrainingSession.genSize-1 && snakeManager.currentSnake.Life <= 0)
            {
                btnStart.IsEnabled = true;    
                btnSave.IsEnabled = true;
                return;                           
            }

            if (!trainingReady)
            {
                timer.Stop();
                return;
            }

            if (updateUi)
            {
                UpdateUi();
            }

            if (draw)
            {
                DrawStuff();
            }

            if (snakeManager.currentSnake.Life <= 0)
            {
                timer.Stop();//pause timer for a moment.
                //ResetGrid();                
                snakeManager.NextSnake();
                timer.Start();
            }

           

            snakeManager.currentSnake.Move();//This will handle updating position, score, life, death, eating food.
            if (Globals.CurrentTrainingSession.newFoodNeeded)
            {
                NewFood();
            }                                              
        }               
                      
        void UpdateChart(ScottPlot.WPF.WpfPlot chart, double[] xVals, double[] yVals, double yLow = 0, double yHigh = 10000)
        {
            chart.Plot.Clear();
            chart.Plot.Axes.SetLimits(xVals[0], xVals.Last(), yLow, yHigh);
            chart.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericFixedInterval(100);
            chart.Plot.Axes.Top.TickGenerator = new ScottPlot.TickGenerators.NumericFixedInterval(1000);
            var sp = chart.Plot.Add.ScatterLine(xVals, yVals);
            sp.Smooth = true;
            sp.FillY = true;
            sp.FillYColor = sp.Color.WithAlpha(0.2);
            chart.Refresh();
        }       

        /// <summary>
        /// Saves a replay for an LSTM. Does not do a copy, you should pass in a hard copy yourself if a copy is needed
        /// </summary>
        /// <param name="saveForThisCell"></param>
        //void SaveReplay(LSTMCell saveForThisCell)
        //{
        //    Replay saveMe = new Replay()
        //    {
        //        GUID = Guid.NewGuid().ToString(),
        //        TrainingGUID = Globals.CurrentTrainingSession.GUID,
        //        SnakeGUID = saveForThisCell.Guid,
        //        Generation = saveForThisCell.generation,               
        //        StartX = saveForThisCell.startingX,
        //        StartY = saveForThisCell.startingY,
        //        FoodLocationsInOrder = saveForThisCell.foodPositions
        //    };

        //    if (FileManager.SaveReplay(saveMe, saveForThisCell) == 1)
        //    {
        //        FileManager.SaveTrainingSession(Globals.CurrentTrainingSession);
        //        FileManager.SaveReplay(saveMe, saveForThisCell);
        //    }
        //}

        private void SldSimSpeed_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!paused)
            {
                timer.Stop();
                timer.Interval = TimeSpan.FromSeconds(sldSimSpeed.Value);
                timer.Start();
            }           
        }

        private void SldSimSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!paused)
            {
                timer.Stop();
                timer.Interval = TimeSpan.FromSeconds(sldSimSpeed.Value);
                timer.Start();
            }
        }

        private void chkDraw_Checked(object sender, RoutedEventArgs e)
        {
            if (chkDraw.IsChecked == true)
            {
                draw = true;
            }
            else
            {
                draw = false;
            }
        }

        private void chkDraw_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chkDraw.IsChecked == true)
            {
                draw = true;
            }
            else
            {
                draw = false;
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStop.IsEnabled = true;
            btnPause.IsEnabled = true;
            btnSave.IsEnabled = false;
            btnStart.IsEnabled = false;

            if (paused)
            {
                paused = false;                   
                return;
            }

            if (stopped)
            {
                stopped = false;
                return;
            }

            PrepareTrainingSession();
            SetUpGrid();
            Startup();

            txtHeight.IsEnabled = false;
            txtWidth.IsEnabled = false;
            txtDeathChance.IsEnabled = false;
            txtMutateChance.IsEnabled = false;

            txtBreedCount.IsEnabled = false;
            txtGenSize.IsEnabled = false;
            txtKeepCount.IsEnabled = false;
            txtMutateCount.IsEnabled = false;
            txtMaxLength.IsEnabled = false;
 

            chkCanWrap.IsEnabled = false;           
            cmbTrainings.IsEnabled = false;
            cmbInputType.IsEnabled = false;

            trainingReady = true;
            timer.Start();
        }

        void PrepareTrainingSession()
        {
            string selectedTraining = cmbTrainings.SelectedItem.ToString();
            if (cmbTrainings.SelectedIndex != 0)
            {
                //first index is a new session, so not first means load an old one.
                Globals.CurrentTrainingSession = FileManager.LoadTrainingSession(selectedTraining);
            }

            if (Globals.CurrentTrainingSession == null)
            {
                Globals.CurrentTrainingSession = new TrainingSession();
                Globals.CurrentTrainingSession.GUID = Guid.NewGuid().ToString();
                Globals.CurrentTrainingSession.inputType = cmbInputType.SelectedIndex + 1;
                Globals.CurrentTrainingSession.canWrap = (bool)chkCanWrap.IsChecked;
                Globals.CurrentTrainingSession.arrayX = int.Parse(txtWidth.Text);
                Globals.CurrentTrainingSession.arrayY = int.Parse(txtHeight.Text);
                Globals.CurrentTrainingSession.randomDeathChance = int.Parse(txtDeathChance.Text);
                Globals.CurrentTrainingSession.mutateChance = int.Parse(txtMutateChance.Text);
                Globals.CurrentTrainingSession.genSize = int.Parse(txtGenSize.Text);
                Globals.CurrentTrainingSession.keepCount = int.Parse(txtKeepCount.Text);
                Globals.CurrentTrainingSession.breedCount = int.Parse(txtBreedCount.Text);
                Globals.CurrentTrainingSession.mutateCount = int.Parse(txtMutateCount.Text);
                Globals.CurrentTrainingSession.maxSegmentLength = int.Parse(txtMaxLength.Text);

                cmbTrainings.Items.Add(Globals.CurrentTrainingSession.GUID);
                cmbTrainings.SelectedIndex = cmbTrainings.Items.Count - 1;
            }
            else
            {
                cmbInputType.SelectedIndex = Globals.CurrentTrainingSession.inputType - 1;
                chkCanWrap.IsChecked = Globals.CurrentTrainingSession.canWrap;
                txtWidth.Text = Globals.CurrentTrainingSession.arrayX.ToString();
                txtHeight.Text = Globals.CurrentTrainingSession.arrayY.ToString();
                txtDeathChance.Text = Globals.CurrentTrainingSession.randomDeathChance.ToString();
                txtMutateChance.Text = Globals.CurrentTrainingSession.mutateChance.ToString();
                txtGenSize.Text = Globals.CurrentTrainingSession.genSize.ToString();
                txtKeepCount.Text = Globals.CurrentTrainingSession.keepCount.ToString();
                txtBreedCount.Text = Globals.CurrentTrainingSession.breedCount.ToString();
                txtMutateCount.Text = Globals.CurrentTrainingSession.mutateCount.ToString();
                txtMaxLength.Text = Globals.CurrentTrainingSession.maxSegmentLength.ToString();


                //thisGenLSTMs = FileManager.LoadLSTMs(Globals.CurrentTrainingSession.GUID);
                //thisGenLSTMs = thisGenLSTMs.OrderByDescending(x => x.fitness).ToList();
                //for (int a = 0; a < Globals.CurrentTrainingSession.keepCount; a++) //recompete
                //{
                //    bestLSTMs.Add(LSTMManager.SoftCopy(thisGenLSTMs[a]));
                //}                
            }

            //set input count for training session for other classes to access instead of baton passing it everywhere.
            switch (Globals.CurrentTrainingSession.inputType)
            {
                case 1:
                    {

                        Globals.CurrentTrainingSession.inputCount = 14;
                        break;
                    }
                case 2:
                    {
                        Globals.CurrentTrainingSession.inputCount = 28;
                        break;
                    }
                case 3:
                    {
                        Globals.CurrentTrainingSession.inputCount = 6 + (Globals.CurrentTrainingSession.arrayX * Globals.CurrentTrainingSession.arrayY);//big array
                        break;
                    }
                default:
                    {
                        Globals.CurrentTrainingSession.inputType = 1;
                        Globals.CurrentTrainingSession.inputCount = 14;
                        break;
                    }
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(blockArray != null && blockArray.GetLength(0) > 0 && blockArray.GetLength(1) > 0)
            {
                resizeTimer.Stop();
                resizeTimer.Start();
            }            
        }

        void ResizeTimerTick(object sender, EventArgs e)
        {
            ResizeGrid();
            resizeTimer.Stop();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            stopped = true;
            sldSimSpeed.IsEnabled = false;
            btnStop.IsEnabled = false;
            btnPause.IsEnabled = false;
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            paused = true;
            sldSimSpeed.IsEnabled = false;
            btnStop.IsEnabled = false;
            btnPause.IsEnabled = false;
            btnSave.IsEnabled = false;
            btnStart.IsEnabled = true;
        }

        private void btnTrainingSession_Click(object sender, RoutedEventArgs e)
        {
            gridTrainingSession.Visibility = Visibility.Visible;
            gridGameData.Visibility = Visibility.Hidden;
            gridCharts.Visibility = Visibility.Hidden;
        }

        private void btnGameData_Click(object sender, RoutedEventArgs e)
        {
            gridTrainingSession.Visibility = Visibility.Hidden;
            gridGameData.Visibility = Visibility.Visible;
            gridCharts.Visibility = Visibility.Hidden;
        }

        private void btnCharts_Click(object sender, RoutedEventArgs e)
        {
            gridTrainingSession.Visibility = Visibility.Hidden;
            gridGameData.Visibility = Visibility.Hidden;
            gridCharts.Visibility = Visibility.Visible;
        }

        private void chkUpdateUi_Checked(object sender, RoutedEventArgs e)
        {
            if (chkUpdateUi.IsChecked == true)
            {
                updateUi = true;
            }
            else
            {
                updateUi = false;
            }
        }

        private void chkUpdateUi_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chkUpdateUi.IsChecked == true)
            {
                updateUi = true;
            }
            else
            {
                updateUi = false;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            FileManager.SaveTrainingSession(Globals.CurrentTrainingSession);
            snakeManager.SaveAllSnakes();
            //FileManager.SaveLSTMs(Globals.CurrentTrainingSession.GUID, thisGenLSTMs);
        }
    }
}
