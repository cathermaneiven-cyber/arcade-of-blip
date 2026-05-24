using UnityEngine;

public class PortalGrab : MonoBehaviour
{
    [Header("Grab Settings")]
    public float grabRange = 12f;
    public float holdForce = 300f;
    public float throwForce = 20f;

    [Header("References")]
    public Transform grabPoint;

    Rigidbody grabbed;
    int originalLayer;
    float rotateSmooth = 12f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (grabbed == null)
                TryGrab();
            else
                Drop();
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

            Quaternion targetRot = grabPoint.rotation;
            grabbed.MoveRotation(Quaternion.Slerp(grabbed.rotation, targetRot, Time.fixedDeltaTime * rotateSmooth));
        }
    }

    void TryGrab()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, grabRange))
        {
            if (hit.rigidbody != null && hit.collider.CompareTag("Grabbable"))
            {
                grabbed = hit.rigidbody;

                // Save original layer
                originalLayer = grabbed.gameObject.layer;

                // Switch to NonCollidable layer
                grabbed.gameObject.layer = LayerMask.NameToLayer("NonCollidable");

                grabbed.useGravity = false;
                grabbed.linearDamping = 10f;
            }
        }
    }

    void Drop()
    {
        if (grabbed == null) return;

        // Restore original layer
        grabbed.gameObject.layer = originalLayer;

        grabbed.useGravity = true;
        grabbed.linearDamping = 0f;
        grabbed = null;
    }

    void Throw()
    {
        if (grabbed == null) return;

        // Restore original layer
        grabbed.gameObject.layer = originalLayer;

        grabbed.useGravity = true;
        grabbed.linearDamping = 0f;
        grabbed.AddForce(transform.forward * throwForce, ForceMode.VelocityChange);
        grabbed = null;
    }
}
