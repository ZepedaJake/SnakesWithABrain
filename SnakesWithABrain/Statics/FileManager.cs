using SnakesWithABrain.Models;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SnakesWithABrain
{
    public static class FileManager
    {
        static XmlSerializer xsTrainingSession = new XmlSerializer(typeof(TrainingSession));
        static XmlSerializer xsReplay = new XmlSerializer(typeof(Replay));
        static string localPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SnakesWithABrain");
        //static string localPath = Path.Combine("C:\\LOCAL ONLY", "SnakesWithABrain");//OLD TESTING
        public static List<LSTMCell> LoadLSTMs(string fileGuid)
        {
            List<LSTMCell> returnMe = new List<LSTMCell>();
            string filePath = Path.Combine(localPath,"LSTMS",fileGuid);
            DirectoryInfo dir = new DirectoryInfo(filePath);
            foreach(FileInfo f in dir.GetFiles())
            {
                using(StreamReader sr = new StreamReader(f.FullName))
                {
                    string data = sr.ReadToEnd();
                    returnMe.Add(new LSTMCell(data));
                }
            }

            return returnMe;
        }

        public static void SaveLSTMs(string fileGuid, List<LSTMCell> saveMe)
        {
            string filePath = Path.Combine(localPath, "LSTMS", fileGuid);
            if (!Directory.Exists(localPath))
            {
                Directory.CreateDirectory(localPath);
            }

            if (!Directory.Exists(filePath) )
            {
                Directory.CreateDirectory(filePath);
            }

            //clear old files
            foreach(FileInfo fi in new DirectoryInfo(filePath).GetFiles())
            {
                fi.Delete();
            }

            foreach (LSTMCell lstm in saveMe) 
            {
                string newFile = Path.Combine(filePath, $"{lstm.Guid}.lstm");
                using (StreamWriter sw = new StreamWriter(newFile))
                {
                    sw.Write(lstm.Save());
                }
            }           
        }

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

        public static int SaveReplay(Replay replay, LSTMCell cell)
        {
            //returns an int
            //0 = good,
            //1 = training session not saved
            //2 = error
            string filePath = Path.Combine(localPath, "TrainingSessions", replay.TrainingGUID);
            if (!Directory.Exists(filePath))
            {
                return 1;                
            }
            filePath = Path.Combine(filePath, "Replays");
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            string fileName = Path.Combine(filePath, $"{replay.GUID}.replay");
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                xsReplay.Serialize(sw, replay);
            }

            fileName = Path.Combine(filePath, $"{cell.Guid}.lstm");
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.Write(cell.Save());
            }
            return 0;
        }
    }
}
