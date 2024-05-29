using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    [System.Serializable]
    public class GameData
    {
        public int meowCount;
        public bool attackEnabled;
        public bool screamEnabled;
        public Vector3 savedPosition;

        //constructor, ran on starting new game
        public GameData()
        {
            this.meowCount = 0;
            savedPosition = Vector3.zero;
        }
    }
}