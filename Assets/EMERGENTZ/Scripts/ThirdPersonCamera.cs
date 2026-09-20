using UnityEngine;
using UnityEngine.InputSystem;

namespace Emergentz
{
    public sealed class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector3 pivotOffset = new Vector3(0f, 1.5f, 0f);
        [SerializeField] float distance = 7f;
        [SerializeField] float sensitivity = 0.12f;
        [SerializeField] float smoothness = 14f;

        float yaw = 25f;
        float pitch = 18f;

        public void SetTarget(Transform value) => target = value;

        void LateUpdate()
        {
            if (target == null) return;

            if (Mouse.current != null && Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 delta = Mouse.current.delta.ReadValue();
                yaw += delta.x * sensitivity;
                pitch = Mathf.Clamp(pitch - delta.y * sensitivity, -10f, 65f);
            }

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + pivotOffset;
            Vector3 desired = pivot - rotation * Vector3.forward * distance;
            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-smoothness * Time.deltaTime));
            transform.rotation = rotation;
        }
    }
}
