using UnityEngine;
using UnityEngine.InputSystem;

namespace Emergentz
{
    [DefaultExecutionOrder(200)]
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
        float nextShot;
        WeaponAim weapon;
        public Vector3 LastShotStart { get; private set; }
        public Vector3 LastShotEnd { get; private set; }
        public bool IsAtCamp => CampfireCheckpoint.Contains(transform.position);

        public float Health => health;
        public float MaxHealth => maxHealth;
        public bool IsAlive => health > 0f;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            viewCamera = Camera.main;
            health = maxHealth;
            weapon = GetComponent<WeaponAim>();
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

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        void LateUpdate()
        {
            if (!IsAlive || PrototypeGameManager.Instance == null || PrototypeGameManager.Instance.RunEnded) return;
            // Camera and animation have finished for this frame before aligning and firing.
            UpdateAim();
            HandleCombat();
        }

        void UpdateAim()
        {
            if (viewCamera == null) return;
            Vector3 forward = Vector3.ProjectOnPlane(viewCamera.transform.forward, Vector3.up);
            if (forward.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(forward);
            if (weapon != null) weapon.Align(GetAimPoint());
        }

        Vector3 GetAimPoint()
        {
            Ray ray = viewCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            return FirstHit(ray, shotRange, out RaycastHit hit) ? hit.point : ray.GetPoint(shotRange);
        }

        bool FirstHit(Ray ray, float distance, out RaycastHit result)
        {
            RaycastHit[] hits = Physics.RaycastAll(ray, distance, ~0, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (a,b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                if (hit.collider.transform.IsChildOf(transform)) continue;
                result = hit; return true;
            }
            result = default; return false;
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

            // Strafe/backpedal relative to camera; movement never turns the rifle away from aim.

            if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            verticalVelocity += gravity * Time.deltaTime;
            controller.Move((planar * moveSpeed + Vector3.up * verticalVelocity) * Time.deltaTime);
        }

        void HandleCombat()
        {
            if (Mouse.current != null && Mouse.current.leftButton.isPressed) TryFire();
        }

        public bool TryFire()
        {
            if (!IsAlive || PrototypeGameManager.Instance.RunEnded || viewCamera == null || Time.time < nextShot || IsAtCamp) return false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            nextShot = Time.time + 0.18f;
            UpdateAim();
            Vector3 aimPoint = GetAimPoint();
            Vector3 start = weapon != null && weapon.Muzzle != null ? weapon.Muzzle.position : transform.position + Vector3.up * 0.5f;
            Vector3 direction = (aimPoint - start).normalized;
            Vector3 end = start + direction * Mathf.Min(shotRange, Vector3.Distance(start, aimPoint));
            Physics.SyncTransforms();
            // Camera picks the target; the barrel ray determines what the bullet can actually hit.
            if (FirstHit(new Ray(start, direction), Vector3.Distance(start, end) + 0.02f, out RaycastHit hit))
            {
                end = hit.point;
                ZombieAgent zombie = hit.collider.GetComponentInParent<ZombieAgent>();
                if (zombie != null) zombie.TakeDamage(shotDamage);
            }
            LastShotStart = start; LastShotEnd = end;
            var tracer = new GameObject("Shot tracer");
            var line = tracer.AddComponent<LineRenderer>();
            line.sharedMaterial = ShotMaterial;
            line.startColor = line.endColor = new Color(1f, 0.75f, 0.15f);
            line.startWidth = 0.045f; line.endWidth = 0.01f;
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            Destroy(tracer, 0.08f);
            return true;
        }

        static Material shotMaterial;
        static Material ShotMaterial => shotMaterial != null ? shotMaterial : shotMaterial = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));

        public void TakeDamage(float amount)
        {
            if (!IsAlive || IsAtCamp) return;
            health = Mathf.Max(0f, health - amount);
            if (!IsAlive) PrototypeGameManager.Instance?.EndRun("THE HORDE TOOK YOU");
        }

        public void HealFully() => health = maxHealth;
    }
}
