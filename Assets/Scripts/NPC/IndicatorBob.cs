using UnityEngine;

namespace constellations
{
    public class IndicatorBob : MonoBehaviour
    {
        private float startPos;
        private float moveTo;
        private const float moveAmount = 0.1f;
        private const float moveTime = 0.5f;

        private void Awake()
        {
            startPos = transform.position.y;
            moveTo = startPos - moveAmount;
        }

        private void OnEnable()
        {
            LeanTween.moveY(gameObject, moveTo, moveTime).setEaseInOutSine().setLoopPingPong();
        }

        private void OnDisable()
        {
            LeanTween.cancel(gameObject);
            transform.position = new Vector2(transform.position.x, startPos);
        }
    }
}