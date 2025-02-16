using UnityEngine;

namespace constellations
{
    public class EventSystemHandler : MonoBehaviour
    {
        public static EventSystemHandler instance;

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