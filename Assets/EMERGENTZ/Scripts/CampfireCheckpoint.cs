using UnityEngine;

namespace Emergentz
{
    public sealed class CampfireCheckpoint : MonoBehaviour
    {
        [SerializeField] float cooldown = 30f;
        float nextUseTime;

        void OnTriggerEnter(Collider other)
        {
            SurvivorController survivor = other.GetComponent<SurvivorController>();
            if (survivor == null || Time.time < nextUseTime) return;

            survivor.HealFully();
            nextUseTime = Time.time + cooldown;
            PrototypeGameManager.Instance?.ShowMessage("CAMP REACHED — HEALTH RESTORED", 3f);
        }
    }
}
