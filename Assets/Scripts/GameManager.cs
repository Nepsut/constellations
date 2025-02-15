using UnityEngine;

namespace constellations
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        public bool inStarRoom = true;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Debug.LogError("Found more than one GameManager, fix this immediately!!");
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}