using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public float speed = 5;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 9;
    public KeyCode runningKey = KeyCode.LeftShift;

    [Header("Camera Bob + Sway")]
    public Transform cameraHolder;
    public float bobFrequency      = 8f;
    public float bobAmplitude      = 0.05f;
    public float runBobMultiplier  = 1.6f;
    public float swayAmplitude     = 0.03f;
    public float idleSwayFrequency = 1.2f;
    public float idleSwayAmplitude = 0.015f;

    [Tooltip("How fast the camera smooths back to rest when stopping")]
    public float returnSpeed = 6f;

    // Set by Crouch each frame so both scripts write to one place
    [HideInInspector] public Vector3 crouchBobOffset;

    Rigidbody _rb;
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    float   bobTimer  = 0f;
    float   idleTimer = 0f;
    bool    wasMoving = false;
    Vector3 camStartPos;
    Vector3 camTargetPos;

    void Awake()
    {
        _rb          = GetComponent<Rigidbody>();
        camStartPos  = cameraHolder.localPosition;
        camTargetPos = camStartPos;
    }

    void FixedUpdate()
    {
        // ── Movement ──────────────────────────────────────────────────────────
        IsRunning = canRun && Input.GetKey(runningKey);
        float targetMovingSpeed = IsRunning ? runSpeed : speed;
        if (speedOverrides.Count > 0)
            targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();

        Vector2 input = new Vector2(
            Input.GetAxis("Horizontal") * targetMovingSpeed,
            Input.GetAxis("Vertical")   * targetMovingSpeed
        );

        _rb.linearVelocity = transform.rotation * new Vector3(
            input.x,
            _rb.linearVelocity.y,
            input.y
        );

        bool isMoving = input.sqrMagnitude > 0.01f;

        // ── Bob / sway target ─────────────────────────────────────────────────
        if (isMoving)
        {
            float freq = bobFrequency * (IsRunning ? runBobMultiplier : 1f);
            float amp  = bobAmplitude  * (IsRunning ? runBobMultiplier : 1f);

            bobTimer  += Time.fixedDeltaTime * freq;
            idleTimer  = 0f;

            camTargetPos = new Vector3(
                camStartPos.x + Mathf.Cos(bobTimer * 0.5f) * swayAmplitude,
                camStartPos.y + Mathf.Sin(bobTimer) * amp,
                camStartPos.z
            );

            wasMoving = true;
        }
        else
        {
            // Seed idle timer from current position so there's no jump
            if (wasMoving)
            {
                float currentOffsetX = cameraHolder.localPosition.x - camStartPos.x;
                idleTimer = Mathf.Asin(Mathf.Clamp(currentOffsetX / Mathf.Max(idleSwayAmplitude, 0.0001f), -1f, 1f)) / idleSwayFrequency;
                wasMoving = false;
            }

            idleTimer += Time.fixedDeltaTime * idleSwayFrequency;

            camTargetPos = new Vector3(
                camStartPos.x + Mathf.Sin(idleTimer) * idleSwayAmplitude,
                camStartPos.y,  // Y returns smoothly to rest via lerp below
                camStartPos.z
            );
        }

        // ── Apply — single write point for bob + crouch offset ────────────────
        // Crouch.cs writes crouchBobOffset each frame; we add it here so
        // only one script ever sets cameraHolder.localPosition
        cameraHolder.localPosition = Vector3.Lerp(
            cameraHolder.localPosition,
            camTargetPos + crouchBobOffset,
            Time.fixedDeltaTime * returnSpeed
        );
    }
}