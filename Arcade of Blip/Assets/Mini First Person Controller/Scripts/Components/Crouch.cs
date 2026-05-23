using UnityEngine;

public class Crouch : MonoBehaviour
{
    public KeyCode key = KeyCode.LeftControl;

    [Header("Slow Movement")]
    public FirstPersonMovement movement;
    public float movementSpeed = 2;

    [Header("Low Head")]
    public Transform headToLower;
    [HideInInspector] public float? defaultHeadYLocalPosition;
    public float crouchYHeadPosition = 1;

    public CapsuleCollider colliderToLower;
    [HideInInspector] public float? defaultColliderHeight;

    [Header("Tween")]
    public float crouchSpeed = 10f;

    [Header("Crouch Bob + Idle Sway")]
    public Transform cameraHolder;
    public float crouchBobFrequency = 4f;
    public float crouchBobAmplitude = 0.02f;
    public float idleSwayFrequency = 1.2f;
    public float idleSwayAmplitude = 0.01f;

    float bobTimer = 0f;
    float idleTimer = 0f;

    float currentHeadY;
    bool headYInitialized = false;

    public bool IsCrouched { get; private set; }
    public event System.Action CrouchStart, CrouchEnd;

    Vector3 camStartPos;

    void Reset()
    {
        movement = GetComponentInParent<FirstPersonMovement>();
        headToLower = movement.GetComponentInChildren<Camera>().transform;
        colliderToLower = movement.GetComponentInChildren<CapsuleCollider>();
    }

    void Start()
    {
        if (cameraHolder != null)
            camStartPos = cameraHolder.localPosition;
    }

    void LateUpdate()
    {
        bool crouchHeld = Input.GetKey(key);

        if (crouchHeld)
        {
            if (!defaultHeadYLocalPosition.HasValue)
            {
                defaultHeadYLocalPosition = headToLower.localPosition.y;
                currentHeadY = defaultHeadYLocalPosition.Value;
                headYInitialized = true;
            }

            currentHeadY = Mathf.Lerp(currentHeadY, crouchYHeadPosition, Time.deltaTime * crouchSpeed);
            headToLower.localPosition = new Vector3(headToLower.localPosition.x, currentHeadY, headToLower.localPosition.z);

            if (!defaultColliderHeight.HasValue)
                defaultColliderHeight = colliderToLower.height;

            float loweringAmount = defaultHeadYLocalPosition.Value - crouchYHeadPosition;
            float targetHeight = Mathf.Max(defaultColliderHeight.Value - loweringAmount, 0);

            colliderToLower.height = Mathf.Lerp(colliderToLower.height, targetHeight, Time.deltaTime * crouchSpeed);
            colliderToLower.center = Vector3.up * colliderToLower.height * .5f;

            if (!IsCrouched)
            {
                IsCrouched = true;
                SetSpeedOverrideActive(true);
                CrouchStart?.Invoke();
            }

            bobTimer += Time.deltaTime * crouchBobFrequency;
            float crouchBob = Mathf.Sin(bobTimer) * crouchBobAmplitude;

            cameraHolder.localPosition = new Vector3(
                camStartPos.x,
                camStartPos.y + crouchBob,
                camStartPos.z
            );
        }
        else
        {
            if (IsCrouched)
            {
                IsCrouched = false;
                SetSpeedOverrideActive(false);
                CrouchEnd?.Invoke();
            }

            currentHeadY = Mathf.Lerp(currentHeadY, defaultHeadYLocalPosition.Value, Time.deltaTime * crouchSpeed);
            headToLower.localPosition = new Vector3(headToLower.localPosition.x, currentHeadY, headToLower.localPosition.z);

            colliderToLower.height = Mathf.Lerp(colliderToLower.height, defaultColliderHeight.Value, Time.deltaTime * crouchSpeed);
            colliderToLower.center = Vector3.up * colliderToLower.height * .5f;

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

    void SetSpeedOverrideActive(bool state)
    {
        if (!movement) return;

        if (state)
        {
            if (!movement.speedOverrides.Contains(SpeedOverride))
                movement.speedOverrides.Add(SpeedOverride);
        }
        else
        {
            if (movement.speedOverrides.Contains(SpeedOverride))
                movement.speedOverrides.Remove(SpeedOverride);
        }
    }

    float SpeedOverride() => movementSpeed;
}
