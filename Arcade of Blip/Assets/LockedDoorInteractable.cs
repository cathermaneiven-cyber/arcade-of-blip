using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LockedDoorInteractable : MonoBehaviour, IInteractable
{
    [Header("Lock")]
    [Tooltip("The door unlocks when this GameObject is active. Disabled after the door opens.")]
    public GameObject requiredItem;
    [Tooltip("Played when the player tries to open the door without the required item.")]
    public AudioClip lockedSound;

    [Header("Door Motion")]
    public float openAngle = 90f;
    public float openDuration = 0.5f;
    public AnimationCurve swingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Sounds")]
    public AudioClip openSound;
    public AudioClip openEndSound;
    [Range(0f, 1f)] public float volume = 1f;

    private bool _isOpen = false;
    private bool _swinging = false;
    private Quaternion _closedRot;
    private Quaternion _openRot;
    private AudioSource _audio;

    void Awake()
    {
        _closedRot = transform.localRotation;
        _openRot = _closedRot * Quaternion.Euler(0f, openAngle, 0f);
        _audio = GetComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.spatialBlend = 1f;
    }

    public void Interact()
    {
        if (_swinging || _isOpen) return;

        if (requiredItem != null && !requiredItem.activeInHierarchy)
        {
            PlayClip(lockedSound);
            return;
        }

        StartCoroutine(SwingDoor());
    }

    IEnumerator SwingDoor()
    {
        _swinging = true;

        PlayClip(openSound);

        float elapsed = 0f;
        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);
            transform.localRotation = Quaternion.Slerp(_closedRot, _openRot, swingCurve.Evaluate(t));
            yield return null;
        }

        transform.localRotation = _openRot;
        _isOpen = true;

        PlayClip(openEndSound);

        if (requiredItem != null)
            requiredItem.SetActive(false);

        _swinging = false;
    }

    void PlayClip(AudioClip clip)
    {
        if (clip == null) return;
        _audio.PlayOneShot(clip, volume);
    }
}