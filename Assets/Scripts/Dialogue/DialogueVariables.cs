using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class DialogueVariables : ScriptableObject, IDataPersistence
    {
        //all variables altered by dialogue should be housed in this class
        //this class should only be talked to via its scriptableobject
        public int kindness;

        public void LoadData(GameData data)
        {
            // this.kindness = data.kindness;
            this.kindness = 3;
        }

        public void SaveData(ref GameData data)
        {
            data.kindness = this.kindness;
        }
    }
}