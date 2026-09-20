using UnityEngine;
namespace Emergentz
{
    public sealed class WeaponAim : MonoBehaviour
    {
        [SerializeField] Transform rifle;
        [SerializeField] Transform muzzle;
        [SerializeField] Vector3 localBarrelAxis;
        public Transform Muzzle => muzzle;
        public Vector3 BarrelDirection => rifle.TransformDirection(localBarrelAxis).normalized;
        public void Configure(Transform gun, Transform tip, Vector3 axis)
        { rifle = gun; muzzle = tip; localBarrelAxis = axis; }
        public void Align(Vector3 point)
        {
            if (rifle == null || muzzle == null) return;
            // Correct the animated gun after the hand pose is evaluated. Iteration accounts
            // for the muzzle moving around the hand pivot as the weapon rotates.
            for (int i = 0; i < 4; i++)
            {
                Vector3 direction = point - muzzle.position;
                if (direction.sqrMagnitude < 0.0001f) return;
                rifle.rotation = Quaternion.FromToRotation(BarrelDirection, direction.normalized) * rifle.rotation;
            }
        }
    }
}
