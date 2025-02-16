using UnityEngine;
using UnityEngine.SceneManagement;

namespace constellations
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        public bool inStarRoom = false;
        public SceneData currentScene;

        private void Awake()
        {
            if (instance != null)
            {
                Debug.Log("Found more than one GameManager, fixing.");
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}