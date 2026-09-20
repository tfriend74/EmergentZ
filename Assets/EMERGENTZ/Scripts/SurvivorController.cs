using UnityEngine;
using UnityEngine.InputSystem;

namespace Emergentz
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class SurvivorController : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 7f;
        [SerializeField] float rotationSpeed = 14f;
        [SerializeField] float gravity = -25f;
        [SerializeField] float maxHealth = 100f;
        [SerializeField] float shotDamage = 40f;
        [SerializeField] float shotRange = 80f;

        CharacterController controller;
        Camera viewCamera;
        float verticalVelocity;
        float health;

        public float Health => health;
        public float MaxHealth => maxHealth;
        public bool IsAlive => health > 0f;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            viewCamera = Camera.main;
            health = maxHealth;
        }

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            if (!IsAlive || PrototypeGameManager.Instance == null || PrototypeGameManager.Instance.RunEnded)
                return;

            HandleMovement();
            HandleCombat();

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        void HandleMovement()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            Vector2 input = Vector2.zero;
            if (keyboard.wKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed) input.y -= 1f;
            if (keyboard.dKey.isPressed) input.x += 1f;
            if (keyboard.aKey.isPressed) input.x -= 1f;
            input = Vector2.ClampMagnitude(input, 1f);

            Transform cameraTransform = viewCamera != null ? viewCamera.transform : transform;
            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            Vector3 planar = forward * input.y + right * input.x;

            if (planar.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(planar), rotationSpeed * Time.deltaTime);

            if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            verticalVelocity += gravity * Time.deltaTime;
            controller.Move((planar * moveSpeed + Vector3.up * verticalVelocity) * Time.deltaTime);
        }

        void HandleCombat()
        {
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame || viewCamera == null)
                return;

            Ray ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, shotRange, ~0, QueryTriggerInteraction.Ignore))
            {
                ZombieAgent zombie = hit.collider.GetComponentInParent<ZombieAgent>();
                if (zombie != null) zombie.TakeDamage(shotDamage);
            }
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive) return;
            health = Mathf.Max(0f, health - amount);
            if (!IsAlive) PrototypeGameManager.Instance?.EndRun("THE HORDE TOOK YOU");
        }

        public void HealFully() => health = maxHealth;
    }
}
