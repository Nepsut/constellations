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
        public int attackBuffs;
        public int knockbackBuffs;
        public Vector3 savedPosition;

        //constructor, ran on starting new game
        public GameData()
        {
            this.meowCount = 0;
            this.attackBuffs = 0;
            this.knockbackBuffs = 0;
            savedPosition = Vector3.zero;
        }
    }
}