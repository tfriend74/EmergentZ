using UnityEngine;

namespace Emergentz
{
    public sealed class CampfireCheckpoint : MonoBehaviour
    {
        static readonly System.Collections.Generic.HashSet<CampfireCheckpoint> camps = new System.Collections.Generic.HashSet<CampfireCheckpoint>();
        public const float SafeRadius = 5f;
        void OnEnable() => camps.Add(this);
        void OnDisable() => camps.Remove(this);
        public static bool Contains(Vector3 point)
        {
            foreach (var camp in camps)
                if (camp != null && Vector3.ProjectOnPlane(point - camp.transform.position, Vector3.up).sqrMagnitude <= SafeRadius * SafeRadius) return true;
            return false;
        }

        void Update()
        {
            var game = PrototypeGameManager.Instance;
            if (game == null || game.RunEnded || game.Survivor == null) return;
            if (Contains(game.Survivor.transform.position)) game.Survivor.HealFully();
        }
    }
}
