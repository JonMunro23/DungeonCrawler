using UnityEngine;

public class TrajectoryLine : MonoBehaviour
{
    [Header("Output")]
    public LineRenderer line;

    [Header("Throw Params")]
    public Transform muzzle;          // where the grenade launches from
    public float throwSpeed = 15f;    // m/s
    public float arcUpBias = 0.05f;   // adds a tiny upward bias to camera forward (0�0.2)

    [Header("Preview Params")]
    public int maxPoints = 60;        // max vertices in the line
    public float timeStep = 0.05f;    // simulation dt
    public float grenadeRadius = 0.12f;
    public LayerMask collisionMask = ~0;

    Camera cam;

    void Awake()
    {
        cam = Camera.main;
        if (!line) line = GetComponent<LineRenderer>();
    }

    void Update()
    {
        // Compute start pose & velocity (aim with camera)
        Vector3 startPos = muzzle ? muzzle.position : transform.position;
        Vector3 dir = (cam.transform.forward + Vector3.up * arcUpBias).normalized;
        Vector3 startVel = dir * throwSpeed;

        DrawTrajectory(startPos, startVel);
    }

    public void DrawTrajectory(Vector3 startPos, Vector3 startVel)
    {
        if (!line) return;

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
                    line.positionCount = count;
                    line.SetPositions(points);
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

        line.positionCount = count;
        line.SetPositions(points);
    }

    // Example throw you might call when clicking/pressing:
    public void Throw(GameObject grenadePrefab)
    {
        Vector3 dir = (cam.transform.forward + Vector3.up * arcUpBias).normalized;
        Vector3 startVel = dir * throwSpeed;

        var grenade = Instantiate(grenadePrefab, muzzle ? muzzle.position : transform.position, Quaternion.identity);
        var rb = grenade.GetComponent<Rigidbody>();
        rb.linearVelocity = startVel; // let physics take it
    }
}
