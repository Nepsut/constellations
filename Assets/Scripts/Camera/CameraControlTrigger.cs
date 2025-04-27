using Cinemachine;
using constellations;
using UnityEngine;
using UnityEditor;

public enum PanDirection
{
    Up,
    Down,
    Left,
    Right
}

public class CameraControlTrigger : MonoBehaviour
{
    public bool swapCameras = false;
    public bool panCamera = false;

    [HideInInspector] public CinemachineVirtualCamera cameraOnLeft;
    [HideInInspector] public CinemachineVirtualCamera cameraOnRight;

    public PanDirection panDirection;
    public float panDistance = 3f;
    public float panTime = 0.35f;

    private Collider2D coll;

    private void Start()
    {
        coll = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (coll == null) return;
        if (collision.CompareTag("Player"))
        {
            if (panCamera)
            {
                //calling pan coroutine from cameramanager.cs
                StartCoroutine(CameraManager.instance.PanCam(panDistance, panTime, panDirection, false));
            }

            if (swapCameras)
            {
                //camera swapping here
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (coll == null) return;
        if (collision.CompareTag("Player"))
        {
            if (panCamera)
            {
                //calling pan coroutine from cameramanager.cs
                StartCoroutine(CameraManager.instance.PanCam(panDistance, panTime, panDirection, true));
            }

            if (swapCameras)
            {
                //camera swapping here
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CameraControlTrigger))]
public class CameraTriggerScriptEditor : Editor
{
    CameraControlTrigger cameraControlTrigger;

    private void OnEnable()
    {
        cameraControlTrigger = (CameraControlTrigger)target;
    }
    public override void OnInspectorGUI()
    {
        cameraControlTrigger.swapCameras = EditorGUILayout.Toggle("Swap Cameras",
            cameraControlTrigger.swapCameras);
        cameraControlTrigger.panCamera = EditorGUILayout.Toggle("Pan Camera",
            cameraControlTrigger.panCamera);

        //this will enable certain fields in inspector based on bools checked in inspector
        if (cameraControlTrigger.swapCameras)
        {
            cameraControlTrigger.cameraOnLeft = EditorGUILayout.ObjectField("Camera On Left",
            cameraControlTrigger.cameraOnLeft, typeof(CinemachineVirtualCamera), true) as CinemachineVirtualCamera;

            cameraControlTrigger.cameraOnRight = EditorGUILayout.ObjectField("Camera On Right",
            cameraControlTrigger.cameraOnRight, typeof(CinemachineVirtualCamera), true) as CinemachineVirtualCamera;
        }
        if (cameraControlTrigger.panCamera)
        {
            cameraControlTrigger.panDirection = (PanDirection)EditorGUILayout.EnumPopup("Camera Pan Direction",
            cameraControlTrigger.panDirection);

            cameraControlTrigger.panDistance = EditorGUILayout.FloatField("Pan Distance",
            cameraControlTrigger.panDistance);

            cameraControlTrigger.panTime = EditorGUILayout.FloatField("Pan Time",
            cameraControlTrigger.panTime);
        }

        //if gui is changed, set dirty flag on cameraControlTrigger or assigned objects will reset
        if (GUI.changed)
        {
            EditorUtility.SetDirty(cameraControlTrigger);
        }
    }
}

#endif