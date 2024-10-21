using System.Collections;
using System.Collections.Generic;
using System.Linq;
using constellations;
using UnityEngine;

public class HitBoxController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private PlayerController controller;
    [HideInInspector] public GameObject targetEnemy { get; private set; }
    [HideInInspector] public bool canAttackEnemy { get; private set; } = false;
    private string[] enemyTags =  new string[]{"Enemy", "Ghost", "Skeleton"};

    void Start()
    {
        controller = player.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position + new Vector3(controller.offset.x, controller.offset.y, 0);
        if (controller.facingRight)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            transform.position = player.transform.position + new Vector3(controller.offset.x, controller.offset.y, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            transform.position = player.transform.position + new Vector3(-controller.offset.x, controller.offset.y, 0);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == null) return;
        if (enemyTags.Contains(collider.gameObject.tag))
        {
            canAttackEnemy = true;
            targetEnemy = collider.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider == null) return;
        if (enemyTags.Contains(collider.gameObject.tag))
        {
            canAttackEnemy = false;
            targetEnemy = null;
        }
    }
}
