using UnityEngine;

namespace constellations
{
    public class CanvasHandler : MonoBehaviour
    {
        public static CanvasHandler instance;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}