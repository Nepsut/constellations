using UnityEngine;

namespace constellations
{
    public class VariableManager : MonoBehaviour
    {
        public static VariableManager instance;
        public GameData tempData;

        private void Awake()
        {
            if (instance != null)
            {
                Debug.Log("Found more than one VariableManager, fixing.");
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
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