using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class ButtonInteractable : MonoBehaviour, IInteractable
{
    [Header("Sound")]
    public List<AudioClip> pressSounds = new List<AudioClip>();
    public List<AudioClip> releaseSounds = new List<AudioClip>();

    [Range(0f, 1f)] public float volume = 1f;

    [Header("Toggle")]
    public bool isToggle = false;

    [Header("Press Animation")]
    public float pressDepth = 0.05f;
    public float pressSpeed = 20f;

    [Header("Actions")]
    public UnityEngine.Events.UnityEvent onPress;
    public UnityEngine.Events.UnityEvent onRelease;

    private AudioSource _audio;
    private Vector3 _restLocalPos;
    private Vector3 _pressedLocalPos;
    private Vector3 _targetLocalPos;
    private bool _isPressed = false;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.spatialBlend = 1f;

        _restLocalPos = transform.localPosition;
        _pressedLocalPos = _restLocalPos + transform.localRotation * new Vector3(0f, 0f, pressDepth);
        _targetLocalPos = _restLocalPos;
    }

    void Update()
    {
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            _targetLocalPos,
            Time.deltaTime * pressSpeed);
    }

    public void Interact()
    {
        if (isToggle)
        {
            _isPressed = !_isPressed;

            if (_isPressed)
                Press();
            else
                Release();
        }
        else
        {
            Press();
            Invoke(nameof(Release), 0.15f);
        }
    }

    void Press()
    {
        _targetLocalPos = _pressedLocalPos;
        PlayRandomClip(pressSounds);
        onPress?.Invoke();
    }

    void Release()
    {
        _targetLocalPos = _restLocalPos;
        PlayRandomClip(releaseSounds);
        onRelease?.Invoke();
    }

    void PlayRandomClip(List<AudioClip> clips)
    {
        if (clips == null || clips.Count == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Count)];
        if (clip == null) return;

        _audio.PlayOneShot(clip, volume);
    }
}