using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerThrowableManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform throwableArmsSpawnTransform;
    [SerializeField] ThrowableArms currentlySelectedThrowableArms;
    PlayerController playerController;

    ThrowableItemData currentlySelectedThrowable;
    Animator currentThrowableAnimator;
    Transform currentThrowableThrowLocation;
    bool isCurrentThrowableActive;
    public bool isThrowableSelectionMenuOpen, isThrowableReadied, isThrowInProgress, isThrowableDrawn;

    [SerializeField] Dictionary<ThrowableItemData, int> availableThrowables = new Dictionary<ThrowableItemData, int>();
    [SerializeField] List<Throwable> manuallyDetonatedThrowables = new List<Throwable>();

    public static event Action<Dictionary<ThrowableItemData, int>, ThrowableItemData> onThrowableSelectionMenuOpened;
    public static event Action onThrowableSelectionMenuClosed;
    public static event Action<ThrowableItemData, int> onFirstThrowableCollected;
    public static event Action<int> onCurrentlySelectedThrowableAmountUpdated;

    [Header("Charging")]
    [SerializeField] bool enableCharging;
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

    Coroutine swapThrowableCoroutine, removeThrowableCoroutine ;


    private void OnEnable()
    {
        ThrowableSelectionButton.onThrowableSelected += OnThrowableSelected;

        PlayerInventoryManager.onThrowableRemoved += OnThrowableRemoved;
    }

    private void OnDisable()
    {
        ThrowableSelectionButton.onThrowableSelected -= OnThrowableSelected;

        PlayerInventoryManager.onThrowableRemoved -= OnThrowableRemoved;
    }

    void OnThrowableSelected(ThrowableItemData selectedThrowable, int throwableAmount)
    {
        if(IsThrowableActive())
        {
            if (swapThrowableCoroutine != null)
            {
                StopCoroutine(swapThrowableCoroutine);
                swapThrowableCoroutine = null;
            }

            swapThrowableCoroutine =  StartCoroutine(SwapThrowable(selectedThrowable));
        }
        else
            SetCurrentlySelectedThrowable(selectedThrowable);
    }

    void OnThrowableRemoved(ThrowableItemData removedThrowable)
    {
        if (removeThrowableCoroutine != null)
        {
            StopCoroutine(removeThrowableCoroutine);
            removeThrowableCoroutine = null;
        }

         removeThrowableCoroutine = StartCoroutine(RemoveThrowable(removedThrowable));
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

    public void Tick()
    {
        // Don’t simulate/preview during an active throw
        if (isThrowInProgress) return;
        if (!isThrowableReadied) return;

        if(enableCharging)
        {
            // Update charge 0..1 over timeToMaxVelocity
            float raw = Mathf.Clamp01((Time.time - readyStartTime) / Mathf.Max(0.0001f, currentlySelectedThrowable.timeToMaxVelocity));
            float eased = (chargeCurve != null) ? chargeCurve.Evaluate(raw) : raw;

            currentCharge01 = eased;
            currentThrowSpeed = Mathf.Lerp(
                Mathf.Min(currentlySelectedThrowable.minThrowVelocity, currentlySelectedThrowable.maxThrowVelocity),
                currentlySelectedThrowable.maxThrowVelocity,
                currentCharge01
            );
        }

        // World-space start pose & velocity
        Vector3 startPos = currentThrowableThrowLocation.position
                         + currentThrowableThrowLocation.forward * grenadeRadius; // small nudge to avoid self-hit
        Vector3 dir = (currentThrowableThrowLocation.forward + Vector3.up * arcUpBias).normalized;
        Vector3 startVel = dir * currentThrowSpeed;

        DrawTrajectory(startPos, startVel);
    }

    public void Init(PlayerController playerController)
    {
        this.playerController = playerController;
    }

    public void AddThrowableToAvailable(ThrowableItemData throwableToAdd, int amountToAdd)
    {
        if(availableThrowables.Count == 0)
        {
            onFirstThrowableCollected?.Invoke(throwableToAdd, amountToAdd);
            SetCurrentlySelectedThrowable(throwableToAdd);
        }

        availableThrowables[throwableToAdd] =
            availableThrowables.TryGetValue(throwableToAdd, out int current)
            ? current + amountToAdd
            : amountToAdd;

        if (throwableToAdd == currentlySelectedThrowable)
            onCurrentlySelectedThrowableAmountUpdated?.Invoke(availableThrowables[throwableToAdd]);
    }

    public IEnumerator RemoveThrowableFromAvailable(ThrowableItemData throwableToRemove, int amountToRemove)
    {
        if (availableThrowables.TryGetValue(throwableToRemove, out int currentAmount))
        {
            availableThrowables[throwableToRemove] = currentAmount - amountToRemove;
        }

        if (throwableToRemove == currentlySelectedThrowable)
        {
            onCurrentlySelectedThrowableAmountUpdated?.Invoke(availableThrowables[throwableToRemove]);

            yield return new WaitForSeconds(0.7f); // wait for throw animation have finished (NEED TO CONVERT TIME TO VARIABLE)

            if (IsThrowableActive())
                if (GetRemainingAmountOfThrowable(throwableToRemove) == 0 && (currentlySelectedThrowable.detonationType != DetonationType.Remote && manuallyDetonatedThrowables.Count == 0))
                {
                    // remove throwable without playing holster animation
                    isCurrentThrowableActive = false;
                    isThrowableReadied = false;
                    CloseThrowableSelectionMenu();
                    SetTrajectoryLineActive(false);
                    SetCurrentThrowableGameObjectActive(false);

                    yield return playerController.playerWeaponManager.DrawCurrentWeapon();
                }

        }
    }

    IEnumerator RemoveThrowable(ThrowableItemData removedThrowable)
    {
        if (removedThrowable == currentlySelectedThrowable)
            if (PlayerInventoryManager.GetRemainingAmountOfItem(currentlySelectedThrowable) == 0)
            {
                yield return HolsterThrowable();
                yield return playerController.playerWeaponManager.DrawCurrentWeapon();
            }

        removeThrowableCoroutine = null;
    }

    public int GetRemainingAmountOfThrowable(ThrowableItemData throwableToCheck)
    {
        if(availableThrowables.TryGetValue(throwableToCheck, out int amountOfAvailableeThrowables))
            return amountOfAvailableeThrowables;

        return 0;
    }

    #region Throwable Selection Menu

    public void OpenThrowableSelectionMenu()
    {
        if(isThrowableReadied || isThrowableSelectionMenuOpen) return;

        isThrowableSelectionMenuOpen = true;
        onThrowableSelectionMenuOpened?.Invoke(availableThrowables, currentlySelectedThrowable);
    }

    public void CloseThrowableSelectionMenu()
    {
        if (!isThrowableSelectionMenuOpen) return;

        isThrowableSelectionMenuOpen = false;
        onThrowableSelectionMenuClosed?.Invoke();
    }

    #endregion

    #region Equipping

    public IEnumerator ToggleEquipThrowable()
    {
        if (currentlySelectedThrowable == null)
            yield break;

        if (!IsThrowableActive())
        {
            yield return EquipThrowable();
        }
        else
        {
            yield return HolsterThrowable();
            yield return playerController.playerWeaponManager.DrawCurrentWeapon();
        }
    }

    IEnumerator EquipThrowable()
    {
        if ((currentlySelectedThrowable.detonationType == DetonationType.Remote && manuallyDetonatedThrowables.Count == 0) && PlayerInventoryManager.GetRemainingAmountOfItem(currentlySelectedThrowable) == 0)
            yield break;

        playerController.playerWeaponManager.CloseAmmoSelectionMenu();
        yield return playerController.playerWeaponManager.HolsterCurrentWeapon();

        SetCurrentThrowableGameObjectActive(true);

        yield return DrawThrowable();
    }

    public IEnumerator DrawThrowable()
    {
        currentThrowableAnimator.Play("Draw");

        yield return new WaitForSeconds(currentlySelectedThrowable.holsterLength);

        isThrowableDrawn = true;
    }

    public IEnumerator HolsterThrowable()
    {
        isThrowableDrawn = false;
        isCurrentThrowableActive = false; //set inactive early to prevent further readying
        isThrowableReadied = false;
        CloseThrowableSelectionMenu();
        SetTrajectoryLineActive(false);
        currentThrowableAnimator.Play("Holster");

        yield return new WaitForSeconds(currentlySelectedThrowable.holsterLength);

        SetCurrentThrowableGameObjectActive(false);
    }


    public IEnumerator SwapThrowable(ThrowableItemData throwableToSwapTo)
    {
        isThrowableReadied = false;
        SetTrajectoryLineActive(false);
        isCurrentThrowableActive = false; //set inactive early to prevent further readying
        currentThrowableAnimator.Play("Holster");

        yield return new WaitForSeconds(currentlySelectedThrowable.holsterLength);

        SetCurrentThrowableGameObjectActive(false);
        SetCurrentlySelectedThrowable(throwableToSwapTo);
        SetCurrentThrowableGameObjectActive(true);

        swapThrowableCoroutine = null;
    }
    #endregion

    #region Readying
    public void ReadyThrowable()
    {
        if (!IsThrowableActive()) return;
        if (isThrowableReadied) return;
        if (isThrowInProgress) return;
        if (!isThrowableDrawn) return;
        if (currentlySelectedThrowable == null) return;
        if (PlayerInventoryManager.GetRemainingAmountOfItem(currentlySelectedThrowable) == 0) return;
        if (PlayerInventoryManager.isInContainer) return;

        isThrowableReadied = true;

        if(enableCharging)
        {
            //start charging
            readyStartTime = Time.time;
            currentCharge01 = 0f;
            currentThrowSpeed = Mathf.Min(currentlySelectedThrowable.minThrowVelocity, currentlySelectedThrowable.maxThrowVelocity);
        }

        currentThrowSpeed = currentlySelectedThrowable.maxThrowVelocity;

        currentThrowableAnimator.Play("Pull_Pin");
        SetTrajectoryLineActive(true);
    }

    public void UnreadyThrowable()
    {
        if (!isThrowableReadied) return;

        StartCoroutine(UseThrowable()); // Currently just yeets throwable instead of dearming
    }
    #endregion

    public void SetCurrentlySelectedThrowable(ThrowableItemData newThrowable)
    {
        currentlySelectedThrowable = newThrowable;
        ThrowableArms arms = Instantiate(newThrowable.throwableArmsPrefab, throwableArmsSpawnTransform);
        currentlySelectedThrowableArms = arms;
        currentThrowableAnimator = arms.GetArmsAnimator();
        currentThrowableThrowLocation = arms.GetArmsThrowLocation();
        arms.gameObject.SetActive(false);
    }

    public IEnumerator UseThrowable()
    {
        if (isThrowInProgress || ThrowableSelectionManager.isThrowableSelectionMenuOpen)
            yield break;

        if (!isThrowableReadied)
        {
            if(currentlySelectedThrowable.detonationType != DetonationType.Remote)
                yield break;

            if (WorldInteractionManager.IsLookingAtInteractable())
                yield break;

            if (manuallyDetonatedThrowables.Count > 0)
            {
                List<Throwable> detonatedThrowables = new List<Throwable>();
                foreach(Throwable throwable in manuallyDetonatedThrowables)
                {
                    if(throwable.IsArmed())
                    {
                        throwable.Explode();
                        detonatedThrowables.Add(throwable);
                    }
                }

                foreach(Throwable detonatedThrowable in detonatedThrowables)
                {
                    manuallyDetonatedThrowables.Remove(detonatedThrowable);
                }
                detonatedThrowables.Clear();
            }

            if (manuallyDetonatedThrowables.Count == 0 && GetRemainingAmountOfThrowable(currentlySelectedThrowable) == 0)
            {
                //holster remoteexplosives
                yield return new WaitForSeconds(0.7f);
                yield return HolsterThrowable();
                yield return playerController.playerWeaponManager.DrawCurrentWeapon();

            }

            yield break;
        }

        isThrowInProgress = true;   // lock immediately
        isThrowableReadied = false; // consume the readied state

        // Lock in speed & direction at release time (before delays)
        float finalSpeed = currentThrowSpeed;
        Vector3 throwDir = (currentThrowableThrowLocation.forward + Vector3.up * arcUpBias).normalized;

        SetTrajectoryLineActive(false);

        currentThrowableAnimator.Play("Throw");
        yield return new WaitForSeconds(currentlySelectedThrowable.throwDelay);

        Throwable clone = Instantiate(
            currentlySelectedThrowable.throwablePrefab,
            currentThrowableThrowLocation.position,
            currentThrowableThrowLocation.rotation
        );

        clone.Throw(finalSpeed * throwDir);
        if(currentlySelectedThrowable.detonationType == DetonationType.Remote)
            manuallyDetonatedThrowables.Add(clone);

        yield return RemoveThrowableFromAvailable(currentlySelectedThrowable, 1);

        if (GetRemainingAmountOfThrowable(currentlySelectedThrowable) > 0 || (currentlySelectedThrowable.detonationType == DetonationType.Remote && manuallyDetonatedThrowables.Count > 0))
        {
            yield return DrawThrowable();
            //yield return new WaitForSeconds(0.767f);
        }

        isThrowInProgress = false;   // unlock
    }

    void SetCurrentThrowableGameObjectActive(bool isActive)
    {
        if (currentlySelectedThrowable == null)
            return;

        currentlySelectedThrowableArms.gameObject.SetActive(isActive);
        isCurrentThrowableActive = isActive;
    }

    public bool IsThrowableActive() => isCurrentThrowableActive;

    #region Trajectory Line

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

    #endregion

    public void Save(ref PlayerSaveData data)
    {
        data.selectedThrowable = currentlySelectedThrowable;
    }

    public void Load(PlayerSaveData data)
    {
        SetCurrentlySelectedThrowable(data.selectedThrowable);
    }
}
