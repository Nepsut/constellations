using Cinemachine;
using UnityEngine;

namespace constellations
{
    public class Constellation : InteractBase
    {
        public PanDirection panDirection;
        public float panDistance = 3f;
        public float panTime = 0.35f;
        private bool pannedUp = false;
        [SerializeField] private Canvas interfaceCanvas;

        private void Awake()
        {
            interfaceCanvas.worldCamera = Camera.main;
        }

        public override void Interact()
        {
            if (pannedUp) return;
            //calling pan coroutine from cameramanager.cs
            StartCoroutine(CameraManager.instance.PanCam(panDistance, panTime, panDirection, false));
            pannedUp = true;
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (collider == null && !pannedUp) return;
            //calling pan coroutine from cameramanager.cs
            StartCoroutine(CameraManager.instance.PanCam(panDistance, panTime, panDirection, true));
            pannedUp = false;
        }
    }
}
