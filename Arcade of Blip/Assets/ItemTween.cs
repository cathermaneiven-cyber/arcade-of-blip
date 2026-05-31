using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class ItemTween : MonoBehaviour
{
    [Header("Tween Points")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Tween Settings")]
    public float tweenSpeed = 8f;
    public bool playOnEnable = true;

    [Header("Sound")]
    public List<AudioClip> pickupSounds = new List<AudioClip>();
    [Range(0f, 1f)] public float volume = 1f;

    private AudioSource _audio;
    private bool _tweening = false;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.spatialBlend = 1f;
    }

    void OnEnable()
    {
        if (!playOnEnable || startPoint == null || endPoint == null) return;

        transform.position = startPoint.position;
        transform.rotation = startPoint.rotation;
        _tweening = true;
        PlayRandomClip();
    }

    void Update()
    {
        if (!_tweening) return;

        transform.position = Vector3.Lerp(
            transform.position,
            endPoint.position,
            Time.deltaTime * tweenSpeed);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            endPoint.rotation,
            Time.deltaTime * tweenSpeed);

        if (Vector3.Distance(transform.position, endPoint.position) < 0.001f)
        {
            transform.position = endPoint.position;
            transform.rotation = endPoint.rotation;
            _tweening = false;
        }
    }

    public void PlayTween()
    {
        if (startPoint == null || endPoint == null) return;

        transform.position = startPoint.position;
        transform.rotation = startPoint.rotation;
        _tweening = true;
        PlayRandomClip();
    }

    void PlayRandomClip()
    {
        if (pickupSounds == null || pickupSounds.Count == 0) return;
        AudioClip clip = pickupSounds[Random.Range(0, pickupSounds.Count)];
        if (clip == null) return;
        _audio.PlayOneShot(clip, volume);
    }
}