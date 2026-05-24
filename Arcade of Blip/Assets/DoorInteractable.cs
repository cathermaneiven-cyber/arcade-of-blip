using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to the Door hinge GameObject.
/// Implements IInteractable so the cursor system calls it automatically.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class DoorInteractable : MonoBehaviour, IInteractable
{
    [Header("Door Motion")]
    [Tooltip("How many degrees the door swings open. Negate to flip direction.")]
    public float openAngle = 90f;

    [Tooltip("Seconds the door takes to open or close")]
    public float openDuration = 0.5f;

    [Tooltip("Easing curve for the swing")]
    public AnimationCurve swingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Sounds")]
    [Tooltip("Played when the door starts opening")]
    public AudioClip openSound;

    [Tooltip("Played when the door starts closing")]
    public AudioClip closeSound;

    [Tooltip("Played when the door finishes opening (e.g. a thud/creak at the end)")]
    public AudioClip openEndSound;

    [Tooltip("Played when the door finishes closing")]
    public AudioClip closeEndSound;

    [Range(0f, 1f)] public float volume = 1f;

    // ── private ───────────────────────────────────────────────────────────────

    private bool        _isOpen   = false;
    private bool        _swinging = false;
    private Quaternion  _closedRot;
    private Quaternion  _openRot;
    private AudioSource _audio;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void Awake()
    {
        _closedRot = transform.localRotation;
        _openRot   = _closedRot * Quaternion.Euler(0f, openAngle, 0f);
        _audio     = GetComponent<AudioSource>();

        _audio.playOnAwake  = false;
        _audio.spatialBlend = 1f; // 3D sound — falls off with distance
    }

    // ── IInteractable ─────────────────────────────────────────────────────────

    public void Interact()
    {
        if (_swinging) return;
        StartCoroutine(SwingDoor());
    }

    // ── Door swing ────────────────────────────────────────────────────────────

    IEnumerator SwingDoor()
    {
        _swinging = true;

        Quaternion from = transform.localRotation;
        Quaternion to   = _isOpen ? _closedRot : _openRot;

        // Play start sound
        AudioClip startClip = _isOpen ? closeSound : openSound;
        PlayClip(startClip);

        float elapsed = 0f;
        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.Clamp01(elapsed / openDuration);
            transform.localRotation = Quaternion.Slerp(from, to, swingCurve.Evaluate(t));
            yield return null;
        }

        transform.localRotation = to;
        _isOpen = !_isOpen;

        // Play end sound (thud / click into frame)
        AudioClip endClip = _isOpen ? openEndSound : closeEndSound;
        PlayClip(endClip);

        _swinging = false;
    }

    void PlayClip(AudioClip clip)
    {
        if (clip == null) return;
        _audio.PlayOneShot(clip, volume);
    }
}