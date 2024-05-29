using System.Collections;
using System.Collections.Generic;
using constellations;
using UnityEngine;

public class HitBoxController : MonoBehaviour
{
    [SerializeField] GameObject player;
    public GameObject targetEnemy { get; private set; }
    public bool canAttackEnemy { get; private set; } = false;

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
        if (collider.gameObject.CompareTag("Enemy"))
        {
            canAttackEnemy = true;
            targetEnemy = collider.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider == null) return;
        if (collider.gameObject.CompareTag("Enemy"))
        {
            canAttackEnemy = false;
            targetEnemy = null;
        }
    }
}
