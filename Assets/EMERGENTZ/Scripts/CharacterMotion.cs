using UnityEngine;
namespace Emergentz
{
    public sealed class CharacterMotion : MonoBehaviour
    {
        Animator animator;
        Vector3 previous;
        void Start() { animator = GetComponent<Animator>(); previous = transform.position; }
        void Update()
        {
            float speed = Vector3.Distance(previous, transform.position) / Mathf.Max(Time.deltaTime, 0.001f);
            previous = transform.position;
            if (animator != null) animator.SetBool("Moving", speed > 0.15f);
        }
    }
}
