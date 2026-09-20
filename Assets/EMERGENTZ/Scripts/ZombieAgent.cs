using UnityEngine;

namespace Emergentz
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class ZombieAgent : MonoBehaviour
    {
        CharacterController controller;
        SurvivorController target;
        float health;
        float speed;
        float damage;
        float nextAttackTime;

        public void Configure(SurvivorController survivor, int level)
        {
            target = survivor;
            health = 55f + level * 8f;
            speed = Mathf.Min(2.3f + level * 0.12f, 5.5f);
            damage = 8f + level * 1.5f;
            transform.localScale = Vector3.one * Random.Range(0.88f, 1.15f);
        }

        void Awake() => controller = GetComponent<CharacterController>();

        void Update()
        {
            if (target == null || !target.IsAlive || PrototypeGameManager.Instance == null || PrototypeGameManager.Instance.RunEnded)
                return;

            Vector3 toTarget = target.transform.position - transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;
            if (distance > 1.45f)
            {
                Vector3 direction = toTarget.normalized;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 8f * Time.deltaTime);
                controller.SimpleMove(direction * speed);
            }
            else if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + 0.85f;
                target.TakeDamage(damage);
            }
        }

        public void TakeDamage(float amount)
        {
            health -= amount;
            if (health > 0f) return;
            PrototypeGameManager.Instance?.RegisterKill();
            Destroy(gameObject);
        }
    }
}
