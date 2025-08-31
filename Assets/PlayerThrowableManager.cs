using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerThrowableManager : MonoBehaviour
{
    PlayerController playerController;
    [SerializeField] Transform throwableArmsSpawnTransform;

    ThrowableItemData currentlySelectedThrowable;
    [SerializeField] ThrowableArms currentlySelectedThrowableArms;
    Animator currentThrowableAnimator;
    Transform currentThrowableThrowLocation;
    bool isCurrentThrowableActive, isThrowableReadied, isThrowInProgress, isThrowableSelectionMenuOpen;

    public static Action<IInventory, ThrowableItemData> onThrowableSelectionMenuOpened;
    public static Action OnThrowableSelectionMenuClosed;

    [Header("Charging")]
    [SerializeField] AnimationCurve chargeCurve = null;    // optional easing; null = linear

    float readyStartTime;     // when charging began
    float currentCharge01;    // 0..1
    float currentThrowSpeed;  // used for preview & final throw

    [Header("Dotted Line")]
    [SerializeField] Material dottedMaterial;              // assign the material with the dash texture
    [SerializeField, Min(0.01f)] float dotSize = 0.35f;    // world meters per pattern repeat (smaller = more dots)
    [SerializeField] float dotScrollSpeed = 1.5f;          // repeats per second moving towards impact
    static readonly int _MainTex = Shader.PropertyToID("_MainTex");
    static readonly int _BaseMap = Shader.PropertyToID("_BaseMap");
    static readonly int _MainTexST = Shader.PropertyToID("_MainTex_ST");
    static readonly int _BaseMapST = Shader.PropertyToID("_BaseMap_ST");

    // Optional: clamp to avoid “zoomed” dots on very short lines
    [SerializeField] bool clampShortLines = true;

    [Header("Output")]
    [SerializeField] LineRenderer trajectoryLine;

    [Header("Throw Params")]
    public float arcUpBias = 0.05f;   // adds a tiny upward bias to camera forward (0–0.2)

    [Header("Preview Params")]
    public int maxPoints = 60;        // max vertices in the line
    public float timeStep = 0.05f;    // simulation dt
    public float grenadeRadius = 0.12f;
    public LayerMask collisionMask = ~0;


    private void OnEnable()
    {
        ThrowableSelectionButton.onThrowableSelected += OnThrowableSelected;
        PlayerInventoryManager.onFirstThrowableCollected += OnFirstThrowableCollected;
    }

    private void OnDisable()
    {
        ThrowableSelectionButton.onThrowableSelected -= OnThrowableSelected;
        PlayerInventoryManager.onFirstThrowableCollected -= OnFirstThrowableCollected;
    }

    void OnFirstThrowableCollected(ThrowableItemData collectedThrowable)
    {
        SetCurrentlySelectedThrowable(collectedThrowable);
    }

    void OnThrowableSelected(ThrowableItemData selectedThrowable)
    {
        SetCurrentlySelectedThrowable(selectedThrowable);
    }

    void Awake()
    {
        if (trajectoryLine)
        {
            trajectoryLine.useWorldSpace = true;
            trajectoryLine.textureMode = LineTextureMode.Tile; // CRITICAL: tile, not stretch
            if (trajectoryLine.material && trajectoryLine.material.mainTexture)
                trajectoryLine.material.mainTexture.wrapMode = TextureWrapMode.Repeat;
        }
    }

    public void Init(PlayerController playerController)
    {
        this.playerController = playerController;
    }

    public void OpenThrowableSelectionMenu()
    {
        isThrowableSelectionMenuOpen = true;
        onThrowableSelectionMenuOpened?.Invoke(playerController.playerInventoryManager, currentlySelectedThrowable);
    }

    public void CloseThrowableSelectionMenu()
    {
        isThrowableSelectionMenuOpen = false;
        OnThrowableSelectionMenuClosed?.Invoke();
    }

    public void SetCurrentlySelectedThrowable(ThrowableItemData newThrowable)
    {
        Debug.Log(newThrowable.itemName);
        currentlySelectedThrowable = newThrowable;
        ThrowableArms arms = Instantiate(newThrowable.throwableArmsPrefab, throwableArmsSpawnTransform);
        currentlySelectedThrowableArms = arms;
        currentThrowableAnimator = arms.GetArmsAnimator();
        currentThrowableThrowLocation = arms.GetArmsThrowLocation();
        arms.gameObject.SetActive(false);
    }

    public async Task EquipThrowable()
    {
        if (currentlySelectedThrowable == null)
            return;

        if (PlayerInventoryManager.GetRemainingAmountOfItem(currentlySelectedThrowable) == 0)
            return;

        await playerController.playerWeaponManager.currentWeapon.HolsterWeapon();
        SetCurrentThrowableActive(true);
    }

    public async Task UnequipThrowable()
    {
        await playerController.playerWeaponManager.currentWeapon.DrawWeapon();
        SetCurrentThrowableActive(false);
    }

    public void ReadyThrowable()
    {
        if (isThrowableReadied) return;

        // Block ready if a throw is in progress or still cooling down
        if (isThrowInProgress) return;

        if (currentlySelectedThrowable == null) return;
        if (PlayerInventoryManager.GetRemainingAmountOfItem(currentlySelectedThrowable) == 0) return;

        isThrowableReadied = true;

        // start charging
        readyStartTime = Time.time;
        currentCharge01 = 0f;
        currentThrowSpeed = Mathf.Min(currentlySelectedThrowable.minThrowVelocity, currentlySelectedThrowable.maxThrowVelocity);

        currentThrowableAnimator.Play("Pull_Pin");
        SetTrajectoryLineActive(true);
    }

    public void UnreadyThrowable()
    {
        if (!isThrowableReadied) return;
        _ = UseThrowable();
    }

    public async Task UseThrowable()
    {
        if (!isThrowableReadied) return;
        if (isThrowInProgress) return;

        isThrowInProgress = true;   // lock immediately
        isThrowableReadied = false; // consume the readied state

        // Lock in speed & direction at release time (before delays)
        float finalSpeed = currentThrowSpeed;
        Vector3 throwDir = (currentThrowableThrowLocation.forward + Vector3.up * arcUpBias).normalized;

        SetTrajectoryLineActive(false);

        currentThrowableAnimator.Play("Throw");
        await Task.Delay((int)(currentlySelectedThrowable.throwDelay * 1000));

        Throwable clone = Instantiate(
            currentlySelectedThrowable.throwablePrefab,
            currentThrowableThrowLocation.position,
            currentThrowableThrowLocation.rotation
        );

        clone.Throw(finalSpeed * throwDir);

        playerController.playerInventoryManager.RemoveThrowableOfType(currentlySelectedThrowable, 1);

        if (PlayerInventoryManager.GetRemainingAmountOfItem(currentlySelectedThrowable) > 0)
        {
            await Task.Delay((int)(0.7f * 1000));
            currentThrowableAnimator.Play("Draw");
            await Task.Delay((int)(0.767f * 1000));
        }
        else
        {
            await Task.Delay((int)(0.7f * 1000));
            await UnequipThrowable();
        }

        isThrowInProgress = false;   // unlock
    }

    void SetCurrentThrowableActive(bool isActive)
    {
        if (currentlySelectedThrowable == null)
            return;

        currentlySelectedThrowableArms.gameObject.SetActive(isActive);
        isCurrentThrowableActive = isActive;
    }

    public bool IsThrowableActive() => isCurrentThrowableActive;

    void Update()
    {
        // Don’t simulate/preview during an active throw
        if (isThrowInProgress) return;
        if (!isThrowableReadied) return;

        // Update charge 0..1 over timeToMaxVelocity
        float raw = Mathf.Clamp01((Time.time - readyStartTime) / Mathf.Max(0.0001f, currentlySelectedThrowable.timeToMaxVelocity));
        float eased = (chargeCurve != null) ? chargeCurve.Evaluate(raw) : raw;

        currentCharge01 = eased;
        currentThrowSpeed = Mathf.Lerp(
            Mathf.Min(currentlySelectedThrowable.minThrowVelocity, currentlySelectedThrowable.maxThrowVelocity),
            currentlySelectedThrowable.maxThrowVelocity,
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
            Vector3 nextPos = pos + vel * timeStep + 0.5f * Physics.gravity * timeStep * timeStep;

            Vector3 seg = nextPos - pos;
            float segLen = seg.magnitude;
            if (segLen > 0f)
            {
                if (Physics.SphereCast(pos, grenadeRadius, seg.normalized, out RaycastHit hit, segLen, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    points[count++] = hit.point;
                    trajectoryLine.positionCount = count;
                    trajectoryLine.SetPositions(points);

                    UpdateDottedUV(points, count);
                    return;
                }
            }

            points[count++] = nextPos;
            vel += Physics.gravity * timeStep;
            pos = nextPos;

            if (count >= maxPoints) break;
        }

        trajectoryLine.positionCount = count;
        trajectoryLine.SetPositions(points);

        UpdateDottedUV(points, count);
    }

    // keeps dot size constant and scrolls them forward
    void UpdateDottedUV(Vector3[] points, int count)
    {
        if (!trajectoryLine) return;
        var mat = trajectoryLine.material; // instance per LR
        if (!mat) return;

        // 1) World length of the polyline
        float totalLen = 0f;
        for (int i = 1; i < count; i++)
            totalLen += Vector3.Distance(points[i - 1], points[i]);

        // 2) How many repeats so one repeat ~= dotSize meters
        float repeatsExact = totalLen / Mathf.Max(0.001f, dotSize);

        // Clamp to at least 1 to avoid zoomed texture on short lines
        float repeats = clampShortLines ? Mathf.Max(1f, repeatsExact) : Mathf.Max(0.0001f, repeatsExact);

        // 3) Scroll towards the impact
        float offsetX = -(Time.time * dotScrollSpeed % 1f);

        // 4) Write to the correct property (URP/HDRP: _BaseMap_ST, Built-in: _MainTex_ST)
        Vector4 st = new Vector4(repeats, 1f, offsetX, 0f);

        if (mat.HasProperty(_BaseMapST))
        {
            mat.SetVector(_BaseMapST, st);
        }
        else if (mat.HasProperty(_MainTexST))
        {
            mat.SetVector(_MainTexST, st);
        }
        else if (mat.HasProperty(_BaseMap))
        {
            mat.SetTextureScale(_BaseMap, new Vector2(repeats, 1f));
            mat.SetTextureOffset(_BaseMap, new Vector2(offsetX, 0f));
        }
        else if (mat.HasProperty(_MainTex))
        {
            mat.mainTextureScale = new Vector2(repeats, 1f);
            mat.mainTextureOffset = new Vector2(offsetX, 0f);
        }

        // Ensure texture can repeat
        var tex = mat.mainTexture;
        if (tex) tex.wrapMode = TextureWrapMode.Repeat;
    }
}
