using Cinemachine;
using UnityEngine;

namespace constellations
{
    public class Constellation : InteractBase
    {
        public CustomInspectorObjects customInspectorObjects;
        private bool pannedUp = false;

        public override void Interact()
        {
            if (pannedUp) return;
            //calling pan coroutine from cameramanager.cs
            StartCoroutine(CameraManager.instance.PanCam(customInspectorObjects.panDistance, customInspectorObjects.panTime,
            customInspectorObjects.panDirection, false));
            pannedUp = true;
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (collider == null && !pannedUp) return;
            //calling pan coroutine from cameramanager.cs
            StartCoroutine(CameraManager.instance.PanCam(customInspectorObjects.panDistance, customInspectorObjects.panTime,
            customInspectorObjects.panDirection, true));
            pannedUp = false;
        }
    }
}
