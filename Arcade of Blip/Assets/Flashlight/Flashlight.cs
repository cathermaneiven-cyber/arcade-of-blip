using UnityEngine;

public class FlashlightToggle : MonoBehaviour
{
    [Header("Flashlight Settings")]
    public Light flashlight;          // Assign your Light component
    public KeyCode toggleKey = KeyCode.F;
    public float toggleCooldown = 0.2f;

    [Header("Optional")]
    public AudioSource toggleSound;   // Assign a sound if you want

    private bool canToggle = true;

    void Start()
    {
        if (flashlight != null)
            flashlight.enabled = false;   // Start OFF
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey) && canToggle)
        {
            StartCoroutine(ToggleFlashlight());
        }
    }

    private System.Collections.IEnumerator ToggleFlashlight()
    {
        canToggle = false;

        // Toggle the light
        flashlight.enabled = !flashlight.enabled;

        // Play sound if assigned
        if (toggleSound != null)
            toggleSound.Play();

        // Cooldown
        yield return new WaitForSeconds(toggleCooldown);
        canToggle = true;
    }
}
