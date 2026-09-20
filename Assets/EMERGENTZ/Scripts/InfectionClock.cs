using UnityEngine;

namespace Emergentz
{
    public sealed class InfectionClock : MonoBehaviour
    {
        [SerializeField] float startingSeconds = 45f;
        [SerializeField] float secondsPerKill = 6f;
        [SerializeField] float maximumSeconds = 90f;

        public float RemainingSeconds { get; private set; }
        public float MaximumSeconds => maximumSeconds;

        void Start() => RemainingSeconds = startingSeconds;

        void Update()
        {
            if (PrototypeGameManager.Instance == null || PrototypeGameManager.Instance.RunEnded) return;
            RemainingSeconds = Mathf.Max(0f, RemainingSeconds - Time.deltaTime);
            if (RemainingSeconds <= 0f) PrototypeGameManager.Instance.EndRun("THE INFECTION WON");
        }

        public void RewardKill() => RemainingSeconds = Mathf.Min(maximumSeconds, RemainingSeconds + secondsPerKill);
    }
}
