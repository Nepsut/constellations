using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace constellations
{
    public class EnemyBase : MonoBehaviour, IDamageable
    {
        [SerializeField] private float health = 100f;
        private bool isDead = false;
        public bool readyForDelete { get; private set; } = false;
        [SerializeField] private float deathDuration = 1f;

        public void TakeDamage(float damage)
        {
            health -= damage;
            if (health <= 0)
            {
                isDead = true;
                StartCoroutine(Death());
            }
        }

        private IEnumerator Death()
        {
            //play death animation here
            yield return new WaitForSeconds(deathDuration);
            readyForDelete = true;      //check this in main enemy script if planning to do stuff on death

            //TEMP DEATH EFFECTS FOR VISUALIZATION
            this.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            Debug.Log(message: $"enemy is super dead");
        }
    }
}
