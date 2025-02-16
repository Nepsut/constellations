using UnityEngine;

namespace constellations
{
    [CreateAssetMenu(menuName = "SceneData")]
    public class SceneData : ScriptableObject
    {
        [field: SerializeField] public AudioClip sceneMusic { get; private set; }

        //Star room should be 1, others are their number +1
        [field: SerializeField] public int sceneID { get; private set; }
        [field: SerializeField] public Vector3 startPosition { get; private set; }
        [field: SerializeField] public bool faceRightOnStart { get; private set; }
        [field: SerializeField] public float lensOrtho { get; private set; }
        [field: SerializeField] public float screenY { get; private set; }
    }
}