using UnityEngine;

namespace constellations
{
    public class CanvasHandler : MonoBehaviour
    {
        public static CanvasHandler instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}