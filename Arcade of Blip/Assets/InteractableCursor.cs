using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to the Cursor UI GameObject.
///
/// Detection strategy (in order):
///   1. RaycastAll along camera forward — finds any tagged object the ray passes through
///   2. SphereCast fallback with minimumSphereRadius for objects slightly off-centre
///
/// Sprite swaps to sprite1 when an interactable is detected AND player is in range.
/// Cursor nudges toward the object's screen-space direction.
/// Click calls IInteractable.Interact() on the target (searches parents then children).
/// </summary>
public class InteractableCursor : MonoBehaviour
{
    [Header("Sprites")]
    public Texture sprite0;
    public Texture sprite1;

    [Header("Detection")]
    [Tooltip("Tag used on interactable objects")]
    public string interactableTag = "Interactable";

    [Tooltip("Minimum sphere radius for the fallback SphereCast — keeps small/flat objects detectable")]
    public float minimumSphereRadius = 0.4f;

    [Tooltip("Maximum look distance to detect an interactable")]
    public float maxDetectionDistance = 6f;

    [Header("Interaction")]
    [Tooltip("Player transform — leave empty to auto-find via 'Player' tag")]
    public Transform player;

    [Tooltip("Must be this close to the interactable to activate")]
    public float interactionRange = 2.5f;

    [Tooltip("Extra distance beyond interactionRange before deactivating — prevents edge flickering")]
    public float hysteresisBuffer = 0.35f;

    [Tooltip("Mouse button to interact (0 = left, 1 = right)")]
    public int interactMouseButton = 0;

    [Header("Tween")]
    public float tweenOffset = 18f;
    public float tweenSpeed = 9f;

    // ── private ───────────────────────────────────────────────────────────────

    private RawImage _rawImage;
    private RectTransform _rectTransform;
    private Camera _cam;

    private Vector2 _baseAnchoredPos;
    private Vector2 _targetAnchoredPos;

    private bool _inRange = false;
    private bool _wasHovering = false;

    private GameObject _currentTarget;
    private Collider _currentCollider;

    private const float EyeHeight = 1.6f;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void Awake()
    {
        _rawImage = GetComponent<RawImage>();
        _rectTransform = GetComponent<RectTransform>();
        _cam = Camera.main;
        _baseAnchoredPos = _rectTransform.anchoredPosition;
        _targetAnchoredPos = _baseAnchoredPos;

        if (player == null)
        {
            GameObject go = GameObject.FindWithTag("Player");
            if (go != null) player = go.transform;
        }
    }

    void Update()
    {
        bool found = TryFindInteractable(out _currentCollider);
        _currentTarget = found ? _currentCollider.gameObject : null;

        UpdateRangeHysteresis(found, _currentCollider);
        HandleSpriteSwap(_inRange);
        UpdateTweenTarget(_inRange, found ? _currentCollider.bounds.center : Vector3.zero);
        SmoothMoveCursor();
        HandleClick(_inRange);
    }

    // ── Detection ─────────────────────────────────────────────────────────────

    bool TryFindInteractable(out Collider col)
    {
        col = null;

        // Stable origin — not affected by head bob
        Vector3 origin = player != null
            ? player.position + Vector3.up * EyeHeight
            : _cam.transform.position;

        Ray ray = new Ray(origin, _cam.transform.forward);

        // 1. RaycastAll — catches any tagged object the ray passes through,
        //    even if something untagged is in front of it
        RaycastHit[] hits = Physics.RaycastAll(ray, maxDetectionDistance);
        float closestDist = float.MaxValue;

        foreach (RaycastHit h in hits)
        {
            if (h.collider.CompareTag(interactableTag) && h.distance < closestDist)
            {
                closestDist = h.distance;
                col = h.collider;
            }
        }

        if (col != null) return true;

        // 2. SphereCast fallback — catches objects slightly off the ray centre
        //    (important for small or flat objects like buttons/cylinders)
        if (Physics.SphereCast(ray, minimumSphereRadius, out RaycastHit sphereHit, maxDetectionDistance))
        {
            if (sphereHit.collider.CompareTag(interactableTag))
            {
                col = sphereHit.collider;
                return true;
            }
        }

        return false;
    }

    // ── Hysteresis range check ────────────────────────────────────────────────

    void UpdateRangeHysteresis(bool found, Collider col)
    {
        if (!found || player == null)
        {
            _inRange = false;
            return;
        }

        float dist = Vector3.Distance(player.position, col.bounds.center);

        if (!_inRange && dist <= interactionRange)
            _inRange = true;
        else if (_inRange && dist > interactionRange + hysteresisBuffer)
            _inRange = false;
    }

    // ── Sprite swap ───────────────────────────────────────────────────────────

    void HandleSpriteSwap(bool inRange)
    {
        if (inRange && !_wasHovering)
        {
            if (sprite1 != null) _rawImage.texture = sprite1;
        }
        else if (!inRange && _wasHovering)
        {
            if (sprite0 != null) _rawImage.texture = sprite0;
        }
        _wasHovering = inRange;
    }

    // ── Tween ─────────────────────────────────────────────────────────────────

    void UpdateTweenTarget(bool inRange, Vector3 worldCentre)
    {
        if (!inRange)
        {
            _targetAnchoredPos = _baseAnchoredPos;
            return;
        }

        Vector3 screenPos = _cam.WorldToScreenPoint(worldCentre);
        Vector2 screenCentre = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 dir = (Vector2)screenPos - screenCentre;

        if (dir.sqrMagnitude < 1f)
        {
            _targetAnchoredPos = _baseAnchoredPos;
            return;
        }

        _targetAnchoredPos = _baseAnchoredPos + dir.normalized * tweenOffset;
    }

    void SmoothMoveCursor()
    {
        _rectTransform.anchoredPosition = Vector2.Lerp(
            _rectTransform.anchoredPosition,
            _targetAnchoredPos,
            Time.deltaTime * tweenSpeed);
    }

    // ── Interaction ───────────────────────────────────────────────────────────

    void HandleClick(bool inRange)
    {
        if (!inRange) return;
        if (!Input.GetMouseButtonDown(interactMouseButton)) return;
        if (_currentTarget == null) return;

        // Search upward first (tag on Handle, script on Door parent)
        IInteractable interactable = _currentTarget.GetComponentInParent<IInteractable>();

        // Then search downward (tag and script both on same object or child)
        if (interactable == null)
            interactable = _currentTarget.GetComponentInChildren<IInteractable>();

        interactable?.Interact();
    }

    // ── Editor gizmos ─────────────────────────────────────────────────────────

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (_cam == null) _cam = Camera.main;
        if (_cam == null) return;

        Vector3 origin = player != null
            ? player.position + Vector3.up * EyeHeight
            : _cam.transform.position;

        // Detection ray
        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.DrawRay(origin, _cam.transform.forward * maxDetectionDistance);

        // Fallback sphere at end of ray
        Gizmos.color = new Color(0f, 1f, 1f, 0.15f);
        Gizmos.DrawWireSphere(origin + _cam.transform.forward * maxDetectionDistance, minimumSphereRadius);

        if (player != null)
        {
            // Activation range
            Gizmos.color = new Color(1f, 0.85f, 0f, 0.2f);
            Gizmos.DrawWireSphere(player.position, interactionRange);

            // Deactivation range (hysteresis band)
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.1f);
            Gizmos.DrawWireSphere(player.position, interactionRange + hysteresisBuffer);
        }
    }
#endif
}