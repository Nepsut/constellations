using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class ClimbableSensor : MonoBehaviour
    {
        [Header("Climb Sensor")]
        private Vector2 climbCheckBox = Vector2.zero;
        [SerializeField] private LayerMask climbable;
        public int canClimb { get; private set; }
        public bool climbing { get; private set; }
        private const float sizeOffset = 0.04f;

        Vector2 _point, _size;
        
        //run this in FixedUpdate of inheriting class if needed
        protected void CheckWall(Vector2 point, Vector2 size, bool forceFalse = false)
        {
            _point = point;
            _size = size;
            //forceFalse is for things like if a certain state blocks climbing
            Collider2D hitRight = Physics2D.OverlapBox(new Vector2(point.x + size.x / 2, point.y),
            new Vector2(sizeOffset, size.y - sizeOffset), 0, climbable);
            Collider2D hitLeft = Physics2D.OverlapBox(new Vector2(point.x - size.x / 2, point.y),
            new Vector2(sizeOffset, size.y - sizeOffset), 0, climbable);

            if (hitRight) canClimb = 1;
            else if (hitLeft) canClimb = 0;
            else canClimb = -1;
        }

        protected void IsClimbing(float horizontal)
        {
            if ((canClimb == 1 && horizontal > 0) || (canClimb == 0 && horizontal < 0))
            {
                climbing = true;
            }
            else
            {
                climbing = false;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(new Vector2(_point.x + _size.x / 2, _point.y),
            new Vector2(sizeOffset, _size.y - sizeOffset));
        }
    }
}