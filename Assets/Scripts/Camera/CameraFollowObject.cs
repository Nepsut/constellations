using System.Collections;
using UnityEngine;

namespace constellations
{
    public class CameraFollowObject : MonoBehaviour
    {
        //init engine variables
        [SerializeField] private GameObject player;
        [SerializeField] private float distance;

        //init const variables
        private const float flipDuration = 0.5f;

        //init other variables
        private bool facingRight;

        // Start is called before the first frame update
        void Start()
        {
            facingRight = player.GetComponent<PlayerController>().facingRight;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            transform.position = player.transform.position;
        }

        public IEnumerator FlipYLerp()
        {
            float startRotation = transform.localEulerAngles.y;
            float endRotation = calcRotation();
            float yRotation = 0f;

            float takenTime = 0f;
            while (takenTime < flipDuration)
            {
                takenTime += Time.deltaTime;

                //lerping rotation
                yRotation = Mathf.Lerp(startRotation, endRotation, (takenTime / flipDuration));
                transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

                yield return null;
            }
        }

        private float calcRotation()
        {
            facingRight = !facingRight;

            if (!facingRight)
            {
                return 180f;
            }
            else
            {
                return 0f;
            }
        }
    }

}