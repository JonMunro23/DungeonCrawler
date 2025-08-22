using System.Threading.Tasks;
using UnityEngine;

public class PlayerThrowableManager : MonoBehaviour
{
    PlayerController playerController;
    [SerializeField] ThrowableItemData tempDefaultThrowableItem;
    [SerializeField] Transform throwableArmaSpawnTransform;

    ThrowableItemData currentlySelectedThrowable;
    [SerializeField] ThrowableArms currentlySelectedThrowableArms;
    Animator currentThrowableAnimator;
    Transform currentThrowableThrowLocation;
    bool isCurrentThrowableActive, isThrowableReadied;

    public void Init(PlayerController playerController)
    {
        this.playerController = playerController;
    }

    void Awake()
    {
        if (trajectoryLine) trajectoryLine.useWorldSpace = true; // make sure we draw in world space
    }

    private void Start()
    {
        SetCurrentlySelectedThrowable(tempDefaultThrowableItem);
    }

    public void SetCurrentlySelectedThrowable(ThrowableItemData newThrowable)
    {
        currentlySelectedThrowable = newThrowable;
        ThrowableArms arms = Instantiate(newThrowable.throwableArmsPrefab, throwableArmaSpawnTransform, currentlySelectedThrowableArms);
        currentlySelectedThrowableArms = arms;
        currentThrowableAnimator = arms.GetArmsAnimator();
        currentThrowableThrowLocation = arms.GetArmsThrowLocation();
        arms.gameObject.SetActive(false);
    }

    public async Task EquipThrowable()
    {
        if (currentlySelectedThrowable == null)
            return;

        await playerController.playerWeaponManager.currentWeapon.HolsterWeapon();
        SetCurrentThrowableActive(true);
    }

    // ===== Charging fields =====
    [Header("Charging")]
    [SerializeField] float minThrowVelocity = 4f;          // tap speed
    [SerializeField] float timeToMaxVelocity = 1.0f;       // seconds to reach max
    [SerializeField] AnimationCurve chargeCurve = null;    // optional easing; null = linear

    float readyStartTime;     // when charging began
    float currentCharge01;    // 0..1
    float currentThrowSpeed;  // used for preview & final throw

    float MaxThrowVelocity => (currentlySelectedThrowable != null && currentlySelectedThrowable.throwVelocity > 0f)
        ? currentlySelectedThrowable.throwVelocity
        : 15f;

    public void ReadyThrowable()
    {
        if (isThrowableReadied) return;

        isThrowableReadied = true;

        // start charging
        readyStartTime = Time.time;
        currentCharge01 = 0f;
        currentThrowSpeed = Mathf.Min(minThrowVelocity, MaxThrowVelocity);

        currentThrowableAnimator.Play("Pull_Pin");
        SetTrajectoryLineActive(true);
        // display throw trajectory while charging
    }

    public void UnreadyThrowable()
    {
        if(!isThrowableReadied) return;

        _ = UseThrowable();
    }

    public async Task UseThrowable()
    {
        if (!isThrowableReadied)
            return;

        // Lock in speed & direction at release time (before delays)
        float finalSpeed = currentThrowSpeed;
        Vector3 throwDir = (currentThrowableThrowLocation.forward + Vector3.up * arcUpBias).normalized;

        isThrowableReadied = false;
        SetTrajectoryLineActive(false);

        currentThrowableAnimator.Play("Throw");
        await Task.Delay((int)(currentlySelectedThrowable.throwDelay * 1000));

        Throwable clone = Instantiate(
            currentlySelectedThrowable.throwablePrefab,
            currentThrowableThrowLocation.position,
            currentThrowableThrowLocation.rotation
        );

        clone.Launch(finalSpeed * throwDir);

        await Task.Delay((int)(0.7f * 1000));
        currentThrowableAnimator.Play("Draw");
        // Assign clone throwData, remove from inventory, etc.
    }

    void SetCurrentThrowableActive(bool isActive)
    {
        if (currentlySelectedThrowable == null)
            return;

        currentlySelectedThrowableArms.gameObject.SetActive(isActive);
        isCurrentThrowableActive = isActive;
    }

    public bool IsThrowableActive() => isCurrentThrowableActive;

    [Header("Output")]
    [SerializeField] LineRenderer trajectoryLine;

    [Header("Throw Params")]
    public float arcUpBias = 0.05f;   // adds a tiny upward bias to camera forward (0–0.2)

    [Header("Preview Params")]
    public int maxPoints = 60;        // max vertices in the line
    public float timeStep = 0.05f;    // simulation dt
    public float grenadeRadius = 0.12f;
    public LayerMask collisionMask = ~0;

    void Update()
    {
        if (!isThrowableReadied) return;

        // Update charge 0..1 over timeToMaxVelocity
        float raw = Mathf.Clamp01((Time.time - readyStartTime) / Mathf.Max(0.0001f, timeToMaxVelocity));
        float eased = (chargeCurve != null) ? chargeCurve.Evaluate(raw) : raw;

        currentCharge01 = eased;
        currentThrowSpeed = Mathf.Lerp(
            Mathf.Min(minThrowVelocity, MaxThrowVelocity),
            MaxThrowVelocity,
            currentCharge01
        );

        // World-space start pose & velocity
        Vector3 startPos = currentThrowableThrowLocation.position
                         + currentThrowableThrowLocation.forward * grenadeRadius; // small nudge to avoid self-hit
        Vector3 dir = (currentThrowableThrowLocation.forward + Vector3.up * arcUpBias).normalized;
        Vector3 startVel = dir * currentThrowSpeed;

        DrawTrajectory(startPos, startVel);
    }

    void SetTrajectoryLineActive(bool isActive)
    {
        if (trajectoryLine)
        {
            trajectoryLine.enabled = isActive;
            if (!isActive) trajectoryLine.positionCount = 0;
        }
    }

    public void DrawTrajectory(Vector3 startPos, Vector3 startVel)
    {
        if (!trajectoryLine) return;

        Vector3[] points = new Vector3[maxPoints];
        int count = 0;

        Vector3 pos = startPos;
        Vector3 vel = startVel;

        points[count++] = pos;

        for (int i = 0; i < maxPoints - 1; i++)
        {
            // Integrate one step
            Vector3 nextPos = pos + vel * timeStep + 0.5f * Physics.gravity * timeStep * timeStep;

            // Segment collision test (sphere cast to approximate grenade radius)
            Vector3 seg = nextPos - pos;
            float segLen = seg.magnitude;
            if (segLen > 0f)
            {
                if (Physics.SphereCast(pos, grenadeRadius, seg.normalized, out RaycastHit hit, segLen, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    points[count++] = hit.point;
                    trajectoryLine.positionCount = count;
                    trajectoryLine.SetPositions(points);
                    return; // stop at impact point
                }
            }

            // No hit: accept step
            points[count++] = nextPos;

            // Update for next iteration
            vel += Physics.gravity * timeStep;
            pos = nextPos;

            if (count >= maxPoints) break;
        }

        trajectoryLine.positionCount = count;
        trajectoryLine.SetPositions(points);
    }

    //// Example alternate throw:
    //public void Throw(GameObject grenadePrefab)
    //{
    //    Vector3 dir = (cam.transform.forward + Vector3.up * arcUpBias).normalized;
    //    Vector3 startVel = dir * currentThrowSpeed; // use charged speed
    //
    //    var grenade = Instantiate(grenadePrefab, muzzle ? muzzle.position : transform.position, Quaternion.identity);
    //    var rb = grenade.GetComponent<Rigidbody>();
    //    rb.velocity = startVel;
    //}
}
