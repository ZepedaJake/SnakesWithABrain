﻿using SnakesWithABrain.Models;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SnakesWithABrain.Statics;
using System.Data;
using SnakesWithABrain.Interfaces;

namespace SnakesWithABrain
{
    public static class FileManager
    {
        static XmlSerializer xsTrainingSession = new XmlSerializer(typeof(TrainingSession));
        static XmlSerializer xsReplay = new XmlSerializer(typeof(Replay));
        static string localPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SnakesWithABrain");
        //static string localPath = Path.Combine("C:\\LOCAL ONLY", "SnakesWithABrain");//OLD TESTING
        //public static List<LSTMCell> LoadLSTMs(string fileGuid)
        //{
        //    List<LSTMCell> returnMe = new List<LSTMCell>();
        //    string filePath = Path.Combine(localPath,"LSTMS",fileGuid);
        //    DirectoryInfo dir = new DirectoryInfo(filePath);
        //    foreach(FileInfo f in dir.GetFiles())
        //    {
        //        using(StreamReader sr = new StreamReader(f.FullName))
        //        {
        //            string data = sr.ReadToEnd();
        //            returnMe.Add(new LSTMCell(data));
        //        }
        //    }

        //    return returnMe;
        //}
        //public static bool SaveNeuralNetwork(INeuralNetwork saveMe)
        //{
        //    string filePath = Path.Combine(localPath, "TrainingSessions", Globals.CurrentTrainingSession.GUID);
        //    string fileName = Path.Combine(filePath, $"NN_{saveMe.Guid}.nn");
        //    using (StreamWriter sw = new StreamWriter(fileName))
        //    {
        //        sw.Write(saveMe.Save());
        //    }

        //    return true;
        //}
        //public static LSTMCell LoadLSTMCell(string guid)
        //{
        //    LSTMCell returnMe = null;
        //    string filePath = Path.Combine(localPath, "TrainingSessions", Globals.CurrentTrainingSession.GUID);
        //    string fileName = Path.Combine(filePath, $"NN_{guid}.nn");
        //    using (StreamReader sr = new StreamReader(fileName))
        //    {
        //        string data = sr.ReadToEnd();
        //        returnMe = new LSTMCell(data);
        //    }
        //    return returnMe;
        //}

        //public static void SaveLSTMs(string fileGuid, List<LSTMCell> saveMe)
        //{
        //    string filePath = Path.Combine(localPath, "LSTMS", fileGuid);
        //    if (!Directory.Exists(localPath))
        //    {
        //        Directory.CreateDirectory(localPath);
        //    }

        //    if (!Directory.Exists(filePath) )
        //    {
        //        Directory.CreateDirectory(filePath);
        //    }

        //    //clear old files
        //    foreach(FileInfo fi in new DirectoryInfo(filePath).GetFiles())
        //    {
        //        fi.Delete();
        //    }

        //    foreach (LSTMCell lstm in saveMe) 
        //    {
        //        string newFile = Path.Combine(filePath, $"{lstm.Guid}.lstm");
        //        using (StreamWriter sw = new StreamWriter(newFile))
        //        {
        //            sw.Write(lstm.Save());
        //        }
        //    }           
        //}

        public static string[] GetTrainingSessions()
        {
            string filePath = Path.Combine(localPath, "TrainingSessions");
            string[] returnMe = new string[0];
            if (Directory.Exists(filePath))
            {
                DirectoryInfo dir = new DirectoryInfo(filePath);
                returnMe = dir.GetDirectories().Select(x => x.Name).ToArray();
            }
            
            
            
            return returnMe;
        }

        public static void SaveTrainingSession(TrainingSession saveMe)
        {
            string filePath = Path.Combine(localPath, "TrainingSessions", saveMe.GUID);
            if (!Directory.Exists(localPath))
            {
                Directory.CreateDirectory(localPath);
            }

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            string fileName = Path.Combine(filePath, $"Session_{saveMe.GUID}.xml");
            using(StreamWriter sw = new StreamWriter(fileName))
            {
                xsTrainingSession.Serialize(sw, saveMe);
            }
        }

        public static TrainingSession LoadTrainingSession(string guid)
        {
            TrainingSession returnMe = new TrainingSession();
            string filePath = Path.Combine(localPath, "TrainingSessions", guid);
            string file = Path.Combine(filePath, $"Session_{guid}.xml");
            if (File.Exists(file))
            {
                using (StreamReader sr = new StreamReader(file)) 
                {
                    returnMe = xsTrainingSession.Deserialize(sr) as TrainingSession;
                }                
            }

            return returnMe;
        }

        public static bool SaveSnake(Snake saveMe, bool isReplay = false)
        {
            string filePath = Path.Combine(localPath, "TrainingSessions", Globals.CurrentTrainingSession.GUID);
            if (isReplay)
            {
                filePath = Path.Combine(localPath, "Replays");
            }
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            string fileName = Path.Combine(filePath, $"Snake_{saveMe.Guid}.snake");


            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.Write(saveMe.Save() + saveMe.NeuralNetwork.Save());
            }            

            return true;
        }    

        public static List<Snake> LoadSnakes()
        {
            string filePath = Path.Combine(localPath, "TrainingSessions", Globals.CurrentTrainingSession.GUID);           
            List<Snake> returnMe = new List<Snake>();
            if (Directory.Exists(filePath))
            {
                string[] files = Directory.GetFiles(filePath, "*.snake");
                int i = 0;
                foreach (string s in files)
                {
                    using (StreamReader sr = new StreamReader(s))
                    {
                        string data = sr.ReadToEnd();
                        Snake newSnake = new Snake(data);
                        returnMe.Add(newSnake);
                        i++;

                    }
                }
            }

            return returnMe;
        }
        
        public static void ClearOldSnakes()
        {
            string filePath = Path.Combine(localPath, "TrainingSessions", Globals.CurrentTrainingSession.GUID);
            if (Directory.Exists(filePath)) 
            {
                foreach(string s in Directory.GetFiles(filePath, "*.snake"))
                {
                    File.Delete(s);
                }
            }
        }
    }
}
