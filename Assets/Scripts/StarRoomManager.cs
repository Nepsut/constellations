using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class StarRoomManager : MonoBehaviour
    {
        [SerializeField] private Transform starHolder;
        private GameObject[] stars = new GameObject[9];
        [SerializeField] private PlayerController playerController;

        // Start is called before the first frame update
        void Start()
        {
            for (int i = 0; i < starHolder.childCount; i++)
            {
                stars[i] = starHolder.GetChild(i).gameObject;
                if (playerController.playerStars >= starHolder.childCount - i)
                {
                    stars[i].SetActive(false);
                    continue;
                }
                stars[i].SetActive(true);
            }
        }
    }
}