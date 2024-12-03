using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace constellations
{
    public class GroundSensor : MonoBehaviour
    {
        private Vector2 groundCheckBox = Vector2.zero;
        [SerializeField] private BoxCollider2D boxCollider;
        [SerializeField] private LayerMask ground;
        public bool grounded { get; private set; } = false;
        private const float colliderOffset = 0.04f;

        void Awake()
        {
            groundCheckBox = new Vector2(boxCollider.size.x - colliderOffset, colliderOffset);
        }

        void Update()
        {
            CheckGround();
        }

        void CheckGround()
        {
            grounded = Physics2D.OverlapBox(transform.position, groundCheckBox, 0f, ground);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(gameObject.transform.position, groundCheckBox);
        }
    }
}