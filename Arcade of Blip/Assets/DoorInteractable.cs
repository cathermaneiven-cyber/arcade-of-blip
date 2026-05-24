using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to the Door hinge GameObject.
/// Implements IInteractable so the cursor system calls it automatically.
/// </summary>
public class DoorInteractable : MonoBehaviour, IInteractable
{
    [Header("Door Motion")]
    [Tooltip("How many degrees the door swings open. Negate to flip direction.")]
    public float openAngle = 90f;

    [Tooltip("Seconds the door takes to open or close")]
    public float openDuration = 0.5f;

    [Tooltip("Easing curve for the swing")]
    public AnimationCurve swingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private bool      _isOpen   = false;
    private bool      _swinging = false;
    private Quaternion _closedRot;
    private Quaternion _openRot;

    void Awake()
    {
        _closedRot = transform.localRotation;
        _openRot   = _closedRot * Quaternion.Euler(0f, openAngle, 0f);
    }

    public void Interact()
    {
        if (_swinging) return;
        StartCoroutine(SwingDoor());
    }

    IEnumerator SwingDoor()
    {
        _swinging = true;
        Quaternion from = transform.localRotation;
        Quaternion to   = _isOpen ? _closedRot : _openRot;
        float elapsed   = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.Clamp01(elapsed / openDuration);
            transform.localRotation = Quaternion.Slerp(from, to, swingCurve.Evaluate(t));
            yield return null;
        }

        transform.localRotation = to;
        _isOpen   = !_isOpen;
        _swinging = false;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
#endif
}