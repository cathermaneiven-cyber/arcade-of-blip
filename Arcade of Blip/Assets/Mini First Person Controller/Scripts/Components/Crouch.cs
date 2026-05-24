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

    [Header("Crouch Bob")]
    public float crouchBobFrequency = 4f;
    public float crouchBobAmplitude = 0.02f;

    float bobTimer = 0f;
    float currentHeadY;

    public bool IsCrouched { get; private set; }
    public event System.Action CrouchStart, CrouchEnd;

    void Reset()
    {
        movement      = GetComponentInParent<FirstPersonMovement>();
        headToLower   = movement.GetComponentInChildren<Camera>().transform;
        colliderToLower = movement.GetComponentInChildren<CapsuleCollider>();
    }

    void Start()
    {
        // Initialise defaults immediately so LateUpdate never reads a null value
        defaultHeadYLocalPosition = headToLower.localPosition.y;
        currentHeadY              = defaultHeadYLocalPosition.Value;
        defaultColliderHeight     = colliderToLower.height;
    }

    void LateUpdate()
    {
        bool crouchHeld = Input.GetKey(key);

        if (crouchHeld)
        {
            // Lerp head down
            currentHeadY = Mathf.Lerp(currentHeadY, crouchYHeadPosition, Time.deltaTime * crouchSpeed);
            headToLower.localPosition = new Vector3(
                headToLower.localPosition.x,
                currentHeadY,
                headToLower.localPosition.z);

            // Shrink collider
            float loweringAmount = defaultHeadYLocalPosition.Value - crouchYHeadPosition;
            float targetHeight   = Mathf.Max(defaultColliderHeight.Value - loweringAmount, 0f);
            colliderToLower.height = Mathf.Lerp(colliderToLower.height, targetHeight, Time.deltaTime * crouchSpeed);
            colliderToLower.center = Vector3.up * colliderToLower.height * 0.5f;

            if (!IsCrouched)
            {
                IsCrouched = true;
                SetSpeedOverrideActive(true);
                CrouchStart?.Invoke();
            }

            // Add crouch bob offset into FirstPersonMovement so they don't fight
            bobTimer += Time.deltaTime * crouchBobFrequency;
            movement.crouchBobOffset = new Vector3(0f, Mathf.Sin(bobTimer) * crouchBobAmplitude, 0f);
        }
        else
        {
            if (IsCrouched)
            {
                IsCrouched = false;
                SetSpeedOverrideActive(false);
                CrouchEnd?.Invoke();
            }

            // Clear crouch bob offset
            movement.crouchBobOffset = Vector3.zero;
            bobTimer = 0f;

            // Lerp head back up
            currentHeadY = Mathf.Lerp(currentHeadY, defaultHeadYLocalPosition.Value, Time.deltaTime * crouchSpeed);
            headToLower.localPosition = new Vector3(
                headToLower.localPosition.x,
                currentHeadY,
                headToLower.localPosition.z);

            // Restore collider
            colliderToLower.height = Mathf.Lerp(colliderToLower.height, defaultColliderHeight.Value, Time.deltaTime * crouchSpeed);
            colliderToLower.center = Vector3.up * colliderToLower.height * 0.5f;
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
            movement.speedOverrides.Remove(SpeedOverride);
        }
    }

    float SpeedOverride() => movementSpeed;
}