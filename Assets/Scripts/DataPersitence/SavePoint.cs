using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace constellations
{
    public class SavePoint : MonoBehaviour, IDataPersistence
    {
        private Vector3 position;
        private const float positionOffset = 0.3f;
        public bool usedSavepoint = false;

        private void Start()
        {
            position = new Vector3(transform.position.x - positionOffset, transform.position.y,
            transform.position.z);
        }

        public void LoadData(GameData data)
        {
            GameObject.FindGameObjectWithTag("Player").transform.position = data.savedPosition;
        }

        public void SaveData(ref GameData data)
        {
            if (usedSavepoint)
            data.savedPosition = position;
        }
    }
}