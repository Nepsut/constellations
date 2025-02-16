using UnityEngine;

namespace constellations
{
    [CreateAssetMenu(menuName = "SceneData")]
    public class SceneData : ScriptableObject
    {
        [field: SerializeField] public AudioClip sceneMusic { get; private set; }

        //IN ORDER OF APPEARANCE, EXCEPT STAR ROOM, WHICH IS 0!!!!
        [field: SerializeField] public int sceneID { get; private set; }
        [field: SerializeField] public Vector3 startPosition { get; private set; }
        [field: SerializeField] public bool faceRightOnStart { get; private set; }
    }
}