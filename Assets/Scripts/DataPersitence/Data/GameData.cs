using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    [System.Serializable]
    public class GameData
    {
        [Header("Player Variables")]
        public int meowCount;
        public bool attackEnabled;
        public bool screamEnabled;
        public int attackBuffs;
        public int knockbackBuffs;
        public Vector3 savedPosition;

        [Header("Dialogue Variables")]
        public int kindness;

        //constructor, ran on starting new game
        public GameData()
        {
            //player variables
            this.meowCount = 0;
            this.attackBuffs = 0;
            this.knockbackBuffs = 0;
            this.attackEnabled = true;
            this.screamEnabled = true;
            this.savedPosition = Vector3.zero;

            //dialogue variables
            this.kindness = 0;
        }
    }
}