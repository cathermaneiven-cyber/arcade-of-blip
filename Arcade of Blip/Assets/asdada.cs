using UnityEngine;

public class PortalGrab : MonoBehaviour
{
    [Header("Grab Settings")]
    public float grabRange  = 12f;
    public float holdForce  = 300f;
    public float throwForce = 20f;

    [Header("References")]
    public Transform grabPoint;

    Rigidbody grabbed;
    int originalLayer;
    float rotateSmooth = 12f;

    const string GrabLayer = "NonCollidable";
    int grabLayerIndex = -1;

    void Start()
    {
        grabLayerIndex = LayerMask.NameToLayer(GrabLayer);

        if (grabLayerIndex == -1)
            Debug.LogError($"[PortalGrab] Layer \"{GrabLayer}\" not found! " +
                           "Go to Edit > Project Settings > Tags and Layers and add it.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (grabbed == null) TryGrab();
            else                 Drop();
        }

        if (grabbed != null && Input.GetMouseButtonDown(0))
            Throw();
    }

    void FixedUpdate()
    {
        if (grabbed != null)
        {
            Vector3 toPoint = grabPoint.position - grabbed.position;
            grabbed.linearVelocity = toPoint * holdForce * Time.fixedDeltaTime;
            grabbed.MoveRotation(Quaternion.Slerp(
                grabbed.rotation, grabPoint.rotation,
                Time.fixedDeltaTime * rotateSmooth));
        }
    }

    void TryGrab()
    {
        // Don't attempt grab if layer isn't set up
        if (grabLayerIndex == -1)
        {
            Debug.LogError($"[PortalGrab] Cannot grab — layer \"{GrabLayer}\" is missing.");
            return;
        }

        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, grabRange))
        {
            if (hit.rigidbody != null && hit.collider.CompareTag("Grabbable"))
            {
                grabbed = hit.rigidbody;
                originalLayer = grabbed.gameObject.layer;       // save
                grabbed.gameObject.layer = grabLayerIndex;      // switch
                grabbed.useGravity = false;
                grabbed.linearDamping = 10f;
            }
        }
    }

    void Drop()
    {
        if (grabbed == null) return;
        grabbed.gameObject.layer = originalLayer;   // restore
        grabbed.useGravity = true;
        grabbed.linearDamping = 0f;
        grabbed = null;
    }

    void Throw()
    {
        if (grabbed == null) return;
        grabbed.gameObject.layer = originalLayer;   // restore
        grabbed.useGravity = true;
        grabbed.linearDamping = 0f;
        grabbed.AddForce(transform.forward * throwForce, ForceMode.VelocityChange);
        grabbed = null;
    }
}