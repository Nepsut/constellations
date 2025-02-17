using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public interface IDataPersistence
    {
        void LoadData(GameData data);
        void SaveData();
    }
}