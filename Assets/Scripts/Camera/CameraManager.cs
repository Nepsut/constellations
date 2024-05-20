using Cinemachine;
using System.Collections;
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

        //init const variables
        private const float fallPanAmount = 0.25f;
        private const float fallPanTime = 0.5f;
        private const float panDownDuration = 0.3f;

        //init other variables
        private float normYPan;
        private float defaultPanDown;
        public float fallSpeedDampThreshold = -10f; //would be a const but playercontroller barked at me
        private Vector2 startOffset;

        public bool YDampLerping { get; private set; } = false;
        public bool crouchPanning { get; private set; } = false;

        public bool PlayerFallLerped = false;

        void Awake()
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
            defaultPanDown = framingTransposer.m_ScreenY;

            //set starting offset of tracked object
            startOffset = framingTransposer.m_TrackedObjectOffset;
        }

        public IEnumerator LerpYAction(bool t_falling)
        {
            YDampLerping = true;

            //def start damp amount 
            float startDampAmount = framingTransposer.m_YDamping;
            float endDampAmount;

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
                takenTime += Time.deltaTime;

                float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, (takenTime / fallPanTime));
                framingTransposer.m_YDamping = lerpedPanAmount;
                //Debug.Log(message: $"looping {takenTime < fallPanTime} takentime {takenTime}");
                yield return null;
            }

            YDampLerping = false;
        }

        //call this on crouch to pan screen down slightly
        public IEnumerator CrouchOffset(bool t_crouching)
        {
            crouchPanning = true;
            float startOffset = framingTransposer.m_ScreenY;
            float endOffset;
            float yOffset;

            if (t_crouching)
            {
                endOffset = 0.35f;
            }
            else
            {
                endOffset = defaultPanDown;
            }

            float takenTime = 0f;
            while (takenTime < panDownDuration)
            {
                takenTime += Time.deltaTime;

                //panning down with lerp
                yOffset = Mathf.Lerp(startOffset, endOffset, (takenTime / panDownDuration));
                framingTransposer.m_ScreenY = yOffset;

                yield return null;
            }

            crouchPanning = false;
        }

        public IEnumerator PanCam(float panDistance, float panTime, PanDirection panDirection, bool panToStart)
        {
            Vector2 endPos = Vector2.zero;
            Vector2 startPos = Vector2.zero;

            //handle pan
            if (!panToStart)
            {
                //set direction and distance
                switch (panDirection)
                {
                    case PanDirection.Up:
                        endPos = Vector2.up;
                        break;
                    case PanDirection.Down:
                        endPos = Vector2.down;
                        break;
                    case PanDirection.Left:
                        endPos = Vector2.left;
                        break;
                    case PanDirection.Right:
                        endPos = Vector2.right;
                        break;
                    default:
                        break;
                }

                endPos *= panDistance;
                startPos = startOffset;
                endPos += startPos;
            }
            else
            {
                startPos = framingTransposer.m_TrackedObjectOffset;
                endPos = startOffset;
            }

            //handle actual camera panning
            float takenTime = 0f;
            while (takenTime < panTime)
            {
                takenTime += Time.deltaTime;

                Vector3 panLerp = Vector3.Lerp(startPos, endPos, (takenTime /  panTime));
                framingTransposer.m_TrackedObjectOffset = panLerp;

                yield return null;
            }
        }
    }
}