using UnityEngine;

namespace constellations
{
    public class IndicatorBob : MonoBehaviour
    {
        //all this class does is handle up-and-down bobbing of an indicator when an npc can be interacted with

        private float startPos;
        private float moveTo;
        private const float moveAmount = 0.1f;
        private const float moveTime = 0.5f;

        private void Awake()
        {
            //grab start position and where we want to move to
            startPos = transform.position.y;
            moveTo = startPos - moveAmount;
        }

        //object is enabled when player enters trigger radius and only then is movement necessary
        private void OnEnable()
        {
            LeanTween.moveY(gameObject, moveTo, moveTime).setEaseInOutSine().setLoopPingPong();
        }

        //stop moving when not visible and go back to starting position
        private void OnDisable()
        {
            LeanTween.cancel(gameObject);
            transform.position = new Vector2(transform.position.x, startPos);
        }
    }
}