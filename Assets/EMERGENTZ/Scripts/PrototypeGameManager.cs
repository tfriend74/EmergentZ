using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Emergentz
{
    public sealed class PrototypeGameManager : MonoBehaviour
    {
        public static PrototypeGameManager Instance { get; private set; }

        [SerializeField] SurvivorController survivor;
        [SerializeField] InfectionClock infectionClock;
        [SerializeField] float secondsBetweenWaves = 18f;
        [SerializeField] float spawnRadius = 24f;
        [SerializeField] GameObject zombieVisual;

        readonly List<ZombieAgent> zombies = new List<ZombieAgent>();
        float nextWaveTime;
        float runTime;
        float messageUntil;
        string message;

        public int Kills { get; private set; }
        public int Wave { get; private set; }
        public bool RunEnded { get; private set; }
        public string EndReason { get; private set; }
        public float RunTime => runTime;
        public string Message => Time.time < messageUntil ? message : string.Empty;
        public SurvivorController Survivor => survivor;
        public InfectionClock Infection => infectionClock;

        void Awake() => Instance = this;

        void Start()
        {
            nextWaveTime = Time.time + 2f;
            ShowMessage("YOU ARE INFECTED. KILLS BUY TIME.", 4f);
        }

        void Update()
        {
            if (RunEnded)
            {
                if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }

            if (survivor.IsAtCamp) { nextWaveTime += Time.deltaTime; return; }
            runTime += Time.deltaTime;
            if (Time.time >= nextWaveTime)
            {
                SpawnWave();
                nextWaveTime = Time.time + secondsBetweenWaves;
            }
        }

        void SpawnWave()
        {
            Wave++;
            int count = 3 + Wave * 2;
            for (int i = 0; i < count; i++) SpawnZombie(i);
            ShowMessage($"WAVE {Wave} — {count} INFECTED", 2.5f);
        }

        void SpawnZombie(int index)
        {
            float angle = (index / Mathf.Max(1f, 3f + Wave * 2f)) * Mathf.PI * 2f + Random.Range(-0.25f, 0.25f);
            float radius = spawnRadius + Random.Range(-4f, 7f);
            Vector3 position = survivor.transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            position.y = 1f;
            if (CampfireCheckpoint.Contains(position)) return;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = $"Zombie_W{Wave}_{index + 1}";
            body.transform.position = position;
            body.GetComponent<Renderer>().material.color = Color.Lerp(new Color(0.22f, 0.42f, 0.18f), new Color(0.45f, 0.08f, 0.06f), Mathf.Clamp01(Wave / 10f));
            CapsuleCollider oldCollider = body.GetComponent<CapsuleCollider>();
            if (oldCollider != null) Destroy(oldCollider);
            CharacterController character = body.AddComponent<CharacterController>();
            character.height = 2f;
            character.radius = 0.45f;
            character.center = Vector3.zero;
            ZombieAgent zombie = body.AddComponent<ZombieAgent>();
            if (zombieVisual != null)
            {
                body.GetComponent<Renderer>().enabled = false;
                Instantiate(zombieVisual, body.transform).transform.localPosition = Vector3.down;
            }
            zombie.Configure(survivor, Wave);
            zombies.Add(zombie);
        }

        public void RegisterKill()
        {
            Kills++;
            infectionClock.RewardKill();
            ShowMessage("+6 SECONDS", 1.1f);
        }

        public void EndRun(string reason)
        {
            if (RunEnded) return;
            RunEnded = true;
            EndReason = reason;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ShowMessage(string value, float duration)
        {
            message = value;
            messageUntil = Time.time + duration;
        }
    }
}
