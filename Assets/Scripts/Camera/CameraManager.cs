using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class CameraManager : MonoBehaviour
    {
        //init engine variables
        public static CameraManager instance;
        private CinemachineFramingTransposer framingTransposer;
        private CinemachineVirtualCamera currentCam;
        [SerializeField] private CinemachineVirtualCamera[] allCameras;

        //init other variables
        private float fallPanAmount = 0.25f;
        private float fallPanTime = 0.25f;
        public float fallSpeedDampThreshold = -15f;
        private float normYPan;

        public bool YDampLerping { get; private set; } = false;

        public bool PlayerFallLerped = false;

        // Start is called before the first frame update
        void Start()
        {
            if (instance == null)
            {
                instance = this;
            }

            for (int i = 0; i < allCameras.Length; i++)
            {
                if (allCameras[i].enabled)
                {
                    //set current active camera
                    currentCam = allCameras[i];

                    //set framing transposer
                    framingTransposer = currentCam.GetCinemachineComponent<CinemachineFramingTransposer>();
                }
            }

            //set yDamp based on inspector value
            normYPan = framingTransposer.m_YDamping;
        }

        public IEnumerator LerpYAction(bool t_falling)
        {
            yield return new WaitForSeconds(0.25f);
            YDampLerping = true;

            //def start damp amount 
            float startDampAmount = framingTransposer.m_YDamping;
            float endDampAmount = 0f;

            //determine end pan
            if (t_falling)
            {
                endDampAmount = fallPanAmount;
                PlayerFallLerped = true;
            }
            else
            {
                endDampAmount = normYPan;
            }

            float takenTime = 0f;
            while (takenTime < fallPanTime)
            {
                takenTime = Time.fixedDeltaTime;

                float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, (takenTime / fallPanTime));
                framingTransposer.m_YDamping = lerpedPanAmount;

                yield return null;
            }

            YDampLerping = false;
        }

    }

}