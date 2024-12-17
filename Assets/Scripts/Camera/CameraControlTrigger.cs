using Cinemachine;
using constellations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraControlTrigger : MonoBehaviour
{
    public CustomInspectorObjects customInspectorObjects;
    private Collider2D coll;

    private void Start()
    {
        coll = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (coll != null)
        {
            if (collision.CompareTag("Player"))
            {
                if (customInspectorObjects.panCamera)
                {
                    //calling pan coroutine from cameramanager.cs
                    StartCoroutine(CameraManager.instance.PanCam(customInspectorObjects.panDistance, customInspectorObjects.panTime,
                    customInspectorObjects.panDirection, false));
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (coll != null)
        {
            if (collision.CompareTag("Player"))
            {
                if (customInspectorObjects.panCamera)
                {
                    //calling pan coroutine from cameramanager.cs
                    StartCoroutine(CameraManager.instance.PanCam(customInspectorObjects.panDistance, customInspectorObjects.panTime,
                    customInspectorObjects.panDirection, true));
                }
            }
        }
    }
}

[System.Serializable]
public class CustomInspectorObjects
{
    public bool swapCameras = false;
    public bool panCamera = false;

    [HideInInspector] public CinemachineVirtualCamera cameraOnLeft;
    [HideInInspector] public CinemachineVirtualCamera cameraOnRight;

    [HideInInspector] public PanDirection panDirection;
    [HideInInspector] public float panDistance = 3f;
    [HideInInspector] public float panTime = 0.35f;
}

public enum PanDirection
{
    Up,
    Down,
    Left,
    Right
}

//[CustomEditor(typeof(CameraControlTrigger))]
//public class MyScriptEditor : Editor
//{
//    CameraControlTrigger cameraControlTrigger;

//    private void OnEnable()
//    {
//        cameraControlTrigger = (CameraControlTrigger)target;
//    }
//    public override void OnInspectorGUI()
//    {
//        DrawDefaultInspector();

//        //this will enable certain fields in inspector based on bools checked in inspector
//        if (cameraControlTrigger.customInspectorObjects.swapCameras)
//        {
//            cameraControlTrigger.customInspectorObjects.cameraOnLeft = EditorGUILayout.ObjectField("Camera On Left",
//            cameraControlTrigger.customInspectorObjects.cameraOnLeft, typeof(CinemachineVirtualCamera), true) as CinemachineVirtualCamera;

//            cameraControlTrigger.customInspectorObjects.cameraOnRight = EditorGUILayout.ObjectField("Camera On Right",
//            cameraControlTrigger.customInspectorObjects.cameraOnRight, typeof(CinemachineVirtualCamera), true) as CinemachineVirtualCamera;
//        }
//        if (cameraControlTrigger.customInspectorObjects.panCamera)
//        {
//            cameraControlTrigger.customInspectorObjects.panDirection = (PanDirection)EditorGUILayout.EnumPopup("Camera Pan Direction",
//            cameraControlTrigger.customInspectorObjects.panDirection);

//            cameraControlTrigger.customInspectorObjects.panDistance = EditorGUILayout.FloatField("Pan Distance",
//            cameraControlTrigger.customInspectorObjects.panDistance);

//            cameraControlTrigger.customInspectorObjects.panTime = EditorGUILayout.FloatField("Pan Time",
//            cameraControlTrigger.customInspectorObjects.panTime);
//        }

//        //if gui is changed, set dirty flag on cameraControlTrigger or assigned objects will reset
//        if (GUI.changed)
//        {
//            EditorUtility.SetDirty(cameraControlTrigger);
//        }
//    }
//}