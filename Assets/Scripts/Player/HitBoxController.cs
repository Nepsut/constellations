using System.Collections;
using System.Collections.Generic;
using System.Linq;
using constellations;
using UnityEngine;

public class HitBoxController : MonoBehaviour
{
    [SerializeField] GameObject player;
    [HideInInspector] public GameObject targetEnemy { get; private set; }
    [HideInInspector] public bool canAttackEnemy { get; private set; } = false;
    private string[] enemyTags =  new string[]{"Enemy", "Ghost", "Skeleton"};

    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position;
        if (player.GetComponent<PlayerController>().facingRight)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
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
