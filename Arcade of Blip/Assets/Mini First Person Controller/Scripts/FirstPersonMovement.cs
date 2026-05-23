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
    public float bobFrequency = 8f;
    public float bobAmplitude = 0.05f;
    public float runBobMultiplier = 1.6f;

    public float swayAmplitude = 0.03f;
    public float idleSwayFrequency = 1.2f;
    public float idleSwayAmplitude = 0.015f;

    Rigidbody rigidbody;
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    float bobTimer = 0f;
    float idleTimer = 0f;
    Vector3 camStartPos;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        camStartPos = cameraHolder.localPosition;
    }

    void FixedUpdate()
    {
        IsRunning = canRun && Input.GetKey(runningKey);

        float targetMovingSpeed = IsRunning ? runSpeed : speed;
        if (speedOverrides.Count > 0)
            targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();

        Vector2 targetVelocity = new Vector2(
            Input.GetAxis("Horizontal") * targetMovingSpeed,
            Input.GetAxis("Vertical") * targetMovingSpeed
        );

        rigidbody.linearVelocity = transform.rotation * new Vector3(
            targetVelocity.x,
            rigidbody.linearVelocity.y,
            targetVelocity.y
        );

        bool isMoving = Mathf.Abs(targetVelocity.x) > 0.1f || Mathf.Abs(targetVelocity.y) > 0.1f;

        if (isMoving)
        {
            float freq = bobFrequency * (IsRunning ? runBobMultiplier : 1f);
            float amp = bobAmplitude * (IsRunning ? runBobMultiplier : 1f);

            bobTimer += Time.deltaTime * freq;

            float verticalBob = Mathf.Sin(bobTimer) * amp;
            float sideSway = Mathf.Cos(bobTimer * 0.5f) * swayAmplitude;

            cameraHolder.localPosition = new Vector3(
                camStartPos.x + sideSway,
                camStartPos.y + verticalBob,
                camStartPos.z
            );
        }
        else
        {
            idleTimer += Time.deltaTime * idleSwayFrequency;

            float idleSway = Mathf.Sin(idleTimer) * idleSwayAmplitude;

            cameraHolder.localPosition = new Vector3(
                camStartPos.x + idleSway,
                camStartPos.y,
                camStartPos.z
            );

            bobTimer = 0f;
        }
    }
}
