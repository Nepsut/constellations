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
        public bool[] playedLevels;
        public int playerStars;
        public Dictionary<string, bool> openedGates;

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
            this.savedPosition = new Vector3(-2.3f, 5.6f, 0);
            this.playerStars = 9;
            this.playedLevels = new bool[9];
            for (int i = 0; i < playedLevels.Length; i++)
            {
                playedLevels[i] = false;
            }

            openedGates = new();

            //dialogue variables
            this.kindness = 0;
        }
    }
}