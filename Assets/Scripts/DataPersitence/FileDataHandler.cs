using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace constellations
{
    public class FileDataHandler
    {
        private string dataDirPath = "";
        private string dataFileName = "";

        public FileDataHandler(string dataDirPath, string dataFileName)
        {
            this.dataDirPath = dataDirPath;
            this.dataFileName = dataFileName;
            if (this.dataFileName == "")
            {
                this.dataFileName = "default";
            }
        }

        public GameData Load()
        {
            //using path.combine to account for differing file systems on some operating systems
            string fullPath = Path.Combine(dataDirPath, "saves", dataFileName);
            Debug.Log(message: $"loading data from {fullPath}");
            GameData loadedData = null;
            if (File.Exists(fullPath))
            {
                try
                {
                    string dataToLoad = "";
                    using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            dataToLoad = reader.ReadToEnd();
                        }
                    }
                    //deserialize loaded JSON file to string
                    loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
                }
                catch (Exception e)
                {
                    Debug.LogError(message: $"Error occured while trying to load data from file: {fullPath}'\n{e}");
                }
            }
            return loadedData;
        }

        public void Save(GameData data)
        {
            //using path.combine to account for differing file systems on some operating systems
            string fullPath = Path.Combine(dataDirPath, "saves", dataFileName);

            try
            {
                //creating directory for saves if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                //serialize GameData object into a JSON file
                string dataToStore = JsonUtility.ToJson(data, true);
                //write data to file
                using (FileStream stream = new FileStream(fullPath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(dataToStore);
                    }
                }
            }
            catch (Exception e)
            {
                    Debug.LogError(message: $"Error occured while trying to save data to file: {fullPath}'\n{e}");
            }
        }
    }
}