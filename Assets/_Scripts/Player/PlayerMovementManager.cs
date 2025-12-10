using System;
using System.Collections;
using UnityEngine;

public class PlayerMovementManager : MonoBehaviour
{
    [Header("Grid")]
    public GridNode currentNode;

    [Header("Movement Settings")]
    [Tooltip("How long it should take to move exactly one tile.")]
    public float baseMoveDuration = 0.2f;
    public float weaponReadiedMoveDurationMultiplier = 2;
    float currentMoveDuration = 0.2f;

    [Header("Continuous Movement (Hold-to-move)")]
    [Tooltip("Enable chaining steps while holding down movement keys.")]
    public bool allowContinuousHold = true;
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode strafeLeftKey = KeyCode.A;
    public KeyCode strafeRightKey = KeyCode.D;
    private bool isBusy;

    [Header("Start Ease")]
    [Tooltip("Fraction of the first tile's duration used to ease-in (0 = none, 0.1 = first 10% of tile).")]
    [Range(0f, 0.5f)]
    public float startEaseFraction = 0.15f;

    [Header("Headbob")]
    [Tooltip("Camera to apply headbob to.")]
    [SerializeField] Vector3 cameraInitialLocalPos;
    public Transform cameraTransform;
    public float headbobAmplitude = 0.05f;              // Vertical bob height
    public float headbobHorizontalAmplitude = 0.03f;    // Left/right sway
    public float headbobFrequency = 8f;                 // Bob speed multiplier
    private float headbobTimer = 0f;

    [Header("Weapon Bob")]
    WeaponMotion weaponMotion;

    // FOOTSTEPS -------------------------------
    [Header("Footsteps")]
    [Tooltip("AudioSource used to play footstep sounds.")]
    [SerializeField] private AudioSource footstepAudioSource;

    [Tooltip("Possible footstep clips to randomly choose from.")]
    [SerializeField] private AudioClip[] footstepClips;

    [Tooltip("Volume for each footstep.")]
    [Range(0f, 1f)]
    [SerializeField] private float footstepVolume = 0.8f;

    [Tooltip("Minimum pitch for randomized footsteps.")]
    [SerializeField] private float footstepMinPitch = 0.95f;

    [Tooltip("Maximum pitch for randomized footsteps.")]
    [SerializeField] private float footstepMaxPitch = 1.05f;

    [Tooltip("Minimum time between two footsteps (safeguard against double-triggers).")]
    [SerializeField] private float footstepCooldown = 0.05f;

    private float _lastFootstepTime = -999f;
    // ----------------------------------------

    private PlayerController playerController;

    public static Action onPlayerMoveStarted;
    public static Action onPlayerMoveEnded;

    private void OnEnable()
    {
        RangedWeapon.onRangedWeaponReadied += OnRangedWeaponReadied;
    }

    private void OnDisable()
    {
        RangedWeapon.onRangedWeaponReadied -= OnRangedWeaponReadied;
    }

    void OnRangedWeaponReadied(bool isWeaponReadied)
    {
        if (isWeaponReadied)
            currentMoveDuration = currentMoveDuration * weaponReadiedMoveDurationMultiplier;
        else
            currentMoveDuration = baseMoveDuration;
    }

    private void Awake()
    {
        weaponMotion = GetComponentInChildren<WeaponMotion>();
    }

    private void Start()
    {
        currentMoveDuration = baseMoveDuration;

        if (currentNode != null)
        {
            transform.position = currentNode.moveToTransform != null
                ? currentNode.moveToTransform.position
                : currentNode.transform.position;
        }
    }

    public void Init(PlayerController playerController)
    {
        this.playerController = playerController;
        cameraInitialLocalPos = cameraTransform.localPosition;
    }

    public void Teleport(Vector3 destination)
    {
        transform.position = destination;
        isBusy = false;
        ResetHeadbob();
        // No footsteps here – teleport should be silent.
    }

    // These are called from PlayerInputHandler (usually on key down)
    public void TryMoveForward()
    {
        TryStartContinuousMove(transform.forward, forwardKey);
    }

    public void TryMoveBackward()
    {
        TryStartContinuousMove(-transform.forward, backwardKey);
    }

    public void TryStrafeLeft()
    {
        TryStartContinuousMove(-transform.right, strafeLeftKey);
    }

    public void TryStrafeRight()
    {
        TryStartContinuousMove(transform.right, strafeRightKey);
    }

    /// <summary>
    /// Starts a continuous movement coroutine. The first tile uses the direction
    /// at the moment of the key-down; subsequent tiles re-evaluate direction
    /// each time based on current input + camera/player facing.
    /// </summary>
    private void TryStartContinuousMove(Vector3 worldDir, KeyCode startKey)
    {
        if (isBusy || currentNode == null)
            return;

        // Flatten & normalise the initial direction
        worldDir.y = 0f;
        if (worldDir.sqrMagnitude < 0.0001f)
            return;

        worldDir.Normalize();

        // Check that at least the first tile in this direction is walkable
        GridNode firstTarget = currentNode.GetNodeInDirection(worldDir);
        if (!IsNodeWalkable(firstTarget))
            return;

        StartCoroutine(ContinuousMoveRoutine(worldDir, startKey));
    }

    /// <summary>
    /// Moves tile-by-tile with constant speed.
    /// First tile:
    ///   - Uses the direction when key was pressed.
    ///   - Has a small ease-in.
    /// Subsequent tiles:
    ///   - Re-evaluate direction based on which movement key is currently held
    ///     and the current camera/player facing.
    /// </summary>
    private IEnumerator ContinuousMoveRoutine(Vector3 firstDir, KeyCode startKey)
    {
        isBusy = true;

        GridNode node = currentNode;
        bool isFirstTile = true;

        while (true)
        {
            Vector3 moveDir;
            KeyCode activeKey = startKey;

            if (isFirstTile)
            {
                moveDir = firstDir;
            }
            else
            {
                if (!GetCurrentMoveDirection(out moveDir, out activeKey))
                    break;
            }

            moveDir.y = 0f;
            if (moveDir.sqrMagnitude < 0.0001f)
                break;

            moveDir.Normalize();

            GridNode targetNode = node.GetNodeInDirection(moveDir);
            if (!IsNodeWalkable(targetNode))
                break;

            onPlayerMoveStarted?.Invoke();
            currentNode.ResetOccupant();
            playerController.SetCurrentOccupiedNode(targetNode);


            Vector3 startPos = transform.position;
            Vector3 endPos = targetNode.moveToTransform != null
                ? targetNode.moveToTransform.position
                : targetNode.transform.position;

            float distance = Vector3.Distance(startPos, endPos);
            if (distance <= Mathf.Epsilon)
            {
                // Degenerate case, accept tile and continue
                transform.position = endPos;
                currentNode.ResetOccupant();
                currentNode = targetNode;
                node = targetNode;

                // No footstep here – we didn't really move.

                if (!allowContinuousHold)
                    break;

                isFirstTile = false;
                continue;
            }

            // --- Speed calculation with optional ease-in on first tile ---
            float easeDuration = 0f;
            if (isFirstTile && startEaseFraction > 0f)
            {
                easeDuration = Mathf.Clamp(currentMoveDuration * startEaseFraction, 0f, currentMoveDuration);
            }

            float baseSpeed;
            if (isFirstTile && easeDuration > 0f)
            {
                baseSpeed = distance / (currentMoveDuration - 0.5f * easeDuration);
            }
            else
            {
                baseSpeed = distance / currentMoveDuration;
            }

            float elapsedTileTime = 0f;

            //track if we've played the mid-step sound for this tile
            bool footstepPlayedThisTile = false;

            // Move towards this tile centre
            while (Vector3.Distance(transform.position, endPos) > 0.001f)
            {
                elapsedTileTime += Time.deltaTime;

                float speed = baseSpeed;

                if (isFirstTile && easeDuration > 0f)
                {
                    float factor = Mathf.Clamp01(elapsedTileTime / easeDuration);
                    speed = baseSpeed * factor;
                }

                // Move player root
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    endPos,
                    speed * Time.deltaTime
                );

                //compute progress along this step and fire at halfway
                float travelled = Vector3.Distance(startPos, transform.position);
                float stepProgress = travelled / distance;

                if (!footstepPlayedThisTile && stepProgress >= 0.5f)
                {
                    footstepPlayedThisTile = true;
                    PlayFootstep();
                }

                // Apply headbob based on current movement speed
                ApplyHeadbob(speed);

                yield return null;
            }

            // Snap precisely to tile
            transform.position = endPos;
            //currentNode.ResetOccupant();
            currentNode = targetNode;
            node = targetNode;


            ResetHeadbob();

            isFirstTile = false;

            if (!allowContinuousHold)
                break;
        }

        onPlayerMoveEnded?.Invoke();

        isBusy = false;
        ResetHeadbob();
    }

    /// <summary>
    /// Decide current movement direction based on which key is held *right now*,
    /// using the current transform orientation. Also returns which key we used.
    /// Priority: forward > backward > strafe left > strafe right.
    /// </summary>
    private bool GetCurrentMoveDirection(out Vector3 dir, out KeyCode key)
    {
        dir = Vector3.zero;
        key = KeyCode.None;

        if (Input.GetKey(forwardKey))
        {
            dir = transform.forward;
            key = forwardKey;
        }
        else if (Input.GetKey(backwardKey))
        {
            dir = -transform.forward;
            key = backwardKey;
        }
        else if (Input.GetKey(strafeLeftKey))
        {
            dir = -transform.right;
            key = strafeLeftKey;
        }
        else if (Input.GetKey(strafeRightKey))
        {
            dir = transform.right;
            key = strafeRightKey;
        }
        else
        {
            return false; // no key held
        }

        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f)
            return false;

        dir.Normalize();
        return true;
    }

    /// <summary>
    /// Basic walkability check – adapt this to your game’s rules.
    /// </summary>
    private bool IsNodeWalkable(GridNode node)
    {
        if (node == null)
            return false;

        if (node.nodeData != null && !node.nodeData.isWalkable)
            return false;

        var occ = node.GetOccupantType();
        if (occ == GridNodeOccupantType.Obstacle ||
            occ == GridNodeOccupantType.NPC)
            return false;

        return true;
    }

    private void ApplyHeadbob(float speed)
    {
        if (cameraTransform == null)
            return;

        // Advance timer based on movement speed
        headbobTimer += Time.deltaTime * (speed * headbobFrequency);

        float bobOffsetY = Mathf.Sin(headbobTimer) * headbobAmplitude;
        float bobOffsetX = Mathf.Cos(headbobTimer * 0.5f) * headbobHorizontalAmplitude;

        cameraTransform.localPosition = cameraInitialLocalPos +
                                        new Vector3(bobOffsetX, bobOffsetY, 0f);

        if (weaponMotion != null)
        {
            weaponMotion.SetWeaponBobOffset(bobOffsetY);
        }
    }

    private void ResetHeadbob()
    {
        if (cameraTransform == null)
            return;

        headbobTimer = 0f;
        cameraTransform.localPosition = cameraInitialLocalPos;
    }

    // FOOTSTEPS -------------------------------
    private void PlayFootstep()
    {
        if (footstepAudioSource == null)
            return;

        if (footstepClips == null || footstepClips.Length == 0)
            return;

        if (Time.time < _lastFootstepTime + footstepCooldown)
            return;

        _lastFootstepTime = Time.time;

        // Random clip & pitch for variation
        int index = UnityEngine.Random.Range(0, footstepClips.Length);
        AudioClip clip = footstepClips[index];

        if (clip == null)
            return;

        float pitch = UnityEngine.Random.Range(footstepMinPitch, footstepMaxPitch);
        footstepAudioSource.pitch = pitch;
        footstepAudioSource.PlayOneShot(clip, footstepVolume);
    }
    // ----------------------------------------
}
