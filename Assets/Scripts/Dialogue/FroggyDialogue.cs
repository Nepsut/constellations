using UnityEngine;

namespace constellations
{
    public class FroggyDialogue : NPCDialogue
    {
        [SerializeField] private GameObject movingPlatform;
        [SerializeField] private float offset;
        [SerializeField] private float moveTime;

        protected override void DoAfterDialogue()
        {
            LeanTween.moveX(movingPlatform, movingPlatform.transform.position.x - offset, moveTime)
            .setEaseInOutSine();
        }
    }
}