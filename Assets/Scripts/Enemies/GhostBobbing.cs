using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class GhostBobbing : MonoBehaviour
    {
        //this class handles the ghost's up-and-down bobbing without disturbing its movement

        private const float startOffset = 0.4f;
        private float moveTo;
        private const float moveAmount = 0.8f;
        private const float moveTime = 1.4f;
        private const float deathFloatDown = 0.6f;
        private bool deathHelper = false;
        private GhostBehavior ghostBehavior;

        private void Awake()
        {
            //grab parent's script
            ghostBehavior = transform.parent.GetComponent<GhostBehavior>();
            //grab start position and where we want to move to
            transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y + startOffset);
            moveTo = transform.localPosition.y - moveAmount;
        }

        private void Start()
        {
            LeanTween.moveLocalY(gameObject, moveTo, moveTime).setEaseInOutSine().setLoopPingPong();
        }

        private void Update()
        {
            if (ghostBehavior.isDead)
            {
                deathHelper = true;
                if (deathHelper)
                {
                    LeanTween.cancel(gameObject);
                    LeanTween.moveLocalY(gameObject, transform.localPosition.y - deathFloatDown, GhostBehavior.deathDuration).setEaseOutSine();
                    deathHelper = false;
                }
            }
        }
    }
}
