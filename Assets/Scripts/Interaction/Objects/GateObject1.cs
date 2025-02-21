using System.Collections;
using UnityEngine;

namespace constellations
{
    public class GateObject : InteractBase, IDataPersistence
    {
        private bool wasOpened = false;
        [SerializeField] private string idString = "gate_1";
        [SerializeField] private string keyString = "gate_key_1";
        [SerializeField] private float moveAmount = 1.4f;
        [SerializeField] private float moveTime = 1.4f;

        public override void Interact()
        {
            if (wasOpened) return;
            if (playerController.inventory.Contains(keyString))
            {
                wasOpened = true;
                LeanTween.moveY(gameObject, transform.position.y + moveAmount, moveTime).
                setEaseInOutSine();
            }
        }

        public void LoadData(GameData data)
        {
            if (data.openedGates.ContainsKey(idString))
                this.wasOpened = data.openedGates[idString];
            else
                this.wasOpened = false;
        }

        public void SaveData(ref GameData data)
        {
            data.openedGates[idString] = this.wasOpened;
        }
    }
}