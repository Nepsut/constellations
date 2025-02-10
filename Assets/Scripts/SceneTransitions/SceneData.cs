using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    [CreateAssetMenu(menuName = "SceneData")]
    public class SceneData : ScriptableObject
    {
        [SerializeField] private AudioClip sceneMusic;

        //IN ORDER OF APPEARANCE, EXCEPT STAR ROOM, WHICH IS 0!!!!
        [field: SerializeField] public int sceneID { get; private set; }
    }
}