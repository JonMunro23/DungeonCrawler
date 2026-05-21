using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCMovementBehaviour
{
    Idle,
    Roam,
    Pursue
}

public class NPCMovementController : MonoBehaviour
{
    // ==========================
    // References
    // ==========================
    NPCController controller;
    NPCVisionSensor vision;
    NPCRoamPlanner roamPlanner;

    // ==========================
    // Movement state
    // ==========================
    [Header("Movement")]
    [SerializeField] bool canMove = true;
    public bool isMoving;

    [SerializeField] List<GridNode> currentPathToNavigate = new List<GridNode>();
    [SerializeField] GridNode currentNavTargetNode, previousTargetNode;

    public GridNode CurrentNavTargetNode => currentNavTargetNode;
    public NPCMovementBehaviour currentMovementBehaviour;

    // ==========================
    // Turning
    // ==========================
    [Header("Turning")]
    public Transform currentOrientation;
    public bool isTurning;

    // ==========================
    // Targets
    // ==========================
    [Header("Targets")]
    [SerializeField] GridNode playerGridNode;

    // ==========================
    // Aggro / Deaggro
    // ==========================
    [Header("Aggro")]
    [Tooltip("If true, NPC never deaggros once it has aggroed.")]
    [SerializeField] bool stickyAggro = true;
    //bool isAggro;

    [Header("Deaggro (Timeout)")]
    [SerializeField] bool enableDeaggroTimeout = true;

    [Tooltip("If player is not detected for this many real-time seconds, NPC deaggros back to Roam (only if stickyAggro is false).")]
    [SerializeField] float loseSightSeconds = 10f;

    [Tooltip("How often to re-check detection while pursuing (seconds). Higher = cheaper, lower = more responsive.")]
    [SerializeField] float deaggroCheckInterval = 0.25f;

    float timeSinceLastDetected;
    float timeSinceLastDeaggroCheck;

    // ==========================
    // Repath safety (prevents recursion stack overflow)
    // ==========================
    bool queueRepathToNextFrame = true;

    bool repathQueued;
    Coroutine repathRoutine;

    // ==========================
    // Unity lifecycle
    // ==========================
    void Awake()
    {
        if (vision == null) vision = GetComponent<NPCVisionSensor>();
        if (roamPlanner == null) roamPlanner = GetComponent<NPCRoamPlanner>();

        if (vision != null && currentOrientation != null)
            vision.SetOrientation(currentOrientation);
    }

    void Update()
    {
        TickDeaggroTimeout();
    }

    private void OnEnable()
    {
        PlayerController.onPlayerOccupiedNodeUpdated += OnPlayerOccupiedNodeUpdated;
        NPCController.onNPCDeath += OnNPCDeath;
        GridNode.onNodeOccupancyUpdated += OnNodeOccupancyUpdated;
    }

    private void OnDisable()
    {
        PlayerController.onPlayerOccupiedNodeUpdated -= OnPlayerOccupiedNodeUpdated;
        NPCController.onNPCDeath -= OnNPCDeath;
        GridNode.onNodeOccupancyUpdated -= OnNodeOccupancyUpdated;

        if (controller != null && controller.healthController != null)
            controller.healthController.onDamaged -= OnDamaged;

        if (vision != null)
            vision.ClearHighlights();

        if (repathRoutine != null)
            StopCoroutine(repathRoutine);
        repathRoutine = null;
        repathQueued = false;
    }

    // ==========================
    // Init / external API
    // ==========================
    public void Init(NPCController controller)
    {
        this.controller = controller;

        if (controller != null && controller.healthController != null)
            controller.healthController.onDamaged += OnDamaged;
    }

    public void BeginMovement() => FindNewPath();

    public void SetSpawnBehaviour(NPCMovementBehaviour spawnBehaviour)
    {
        currentMovementBehaviour = spawnBehaviour;

        //if (spawnBehaviour != NPCMovementBehaviour.Pursue && !stickyAggro)
        //    isAggro = false;

        if (spawnBehaviour != NPCMovementBehaviour.Roam && roamPlanner != null)
            roamPlanner.CancelDestination();

        ResetDeaggroTimer();
    }

    // ==========================
    // Event handlers
    // ==========================
    void OnNPCDeath(NPCController deadNPC)
    {
        if (deadNPC == controller) return;

        if (currentMovementBehaviour == NPCMovementBehaviour.Pursue)
            FindNewPath();
    }

    void OnPlayerOccupiedNodeUpdated(GridNode newNode)
    {
        playerGridNode = newNode;

        EvaluateDetectionAndMaybeAggro();

        if (currentMovementBehaviour == NPCMovementBehaviour.Pursue)
            FindNewPath();
    }

    void OnNodeOccupancyUpdated()
    {
        if (isMoving || isTurning) return;

        if (currentMovementBehaviour == NPCMovementBehaviour.Pursue)
            FindNewPath();

        if (currentMovementBehaviour == NPCMovementBehaviour.Roam && roamPlanner != null)
            roamPlanner.InvalidateIfBlocked();
    }

    void OnDamaged(int damage, DamageType damageType, bool isCrit)
    {
        if (!canMove) return;

        AggroToPursue();
        FindNewPath();
    }

    // ==========================
    // Aggro / Deaggro logic
    // ==========================
    void EvaluateDetectionAndMaybeAggro()
    {
        if (vision == null || !vision.Enabled) return;
        if (controller == null || controller.currentlyOccupiedGridnode == null) return;
        if (playerGridNode == null) return;

        bool detected = vision.IsPlayerDetected(controller.currentlyOccupiedGridnode, playerGridNode);

        if (detected)
        {
            if (currentMovementBehaviour == NPCMovementBehaviour.Pursue)
            {
                ResetDeaggroTimer();
                return;
            }

            AggroToPursue();
        }
    }

    void TickDeaggroTimeout()
    {
        if (!enableDeaggroTimeout) return;
        if (stickyAggro) return;
        if (currentMovementBehaviour != NPCMovementBehaviour.Pursue) return;

        timeSinceLastDetected += Time.deltaTime;

        timeSinceLastDeaggroCheck += Time.deltaTime;
        if (timeSinceLastDeaggroCheck < deaggroCheckInterval)
            return;

        timeSinceLastDeaggroCheck = 0f;

        if (vision == null || !vision.Enabled) return;
        if (controller == null || controller.currentlyOccupiedGridnode == null) return;
        if (playerGridNode == null) return;

        bool detectedNow = vision.IsPlayerDetected(controller.currentlyOccupiedGridnode, playerGridNode);

        if (detectedNow)
        {
            ResetDeaggroTimer();
            return;
        }

        if (timeSinceLastDetected >= loseSightSeconds)
            DeaggroToRoam();
    }

    void AggroToPursue()
    {
        //isAggro = true;
        currentMovementBehaviour = NPCMovementBehaviour.Pursue;

        if (roamPlanner != null) roamPlanner.CancelDestination();
        currentPathToNavigate?.Clear();

        ResetDeaggroTimer();
    }

    void DeaggroToRoam()
    {
        //isAggro = false;
        currentMovementBehaviour = NPCMovementBehaviour.Roam;

        if (currentPathToNavigate != null)
            currentPathToNavigate.Clear();

        if (roamPlanner != null) roamPlanner.CancelDestination();

        ResetDeaggroTimer();
        FindNewPath();
    }

    void ResetDeaggroTimer()
    {
        timeSinceLastDetected = 0f;
        timeSinceLastDeaggroCheck = 0f;
    }

    // ==========================
    // Repath safety helpers
    // ==========================
    void QueueRepath()
    {
        if (!queueRepathToNextFrame)
        {
            FindNewPath();
            return;
        }

        if (repathQueued) return;
        repathQueued = true;

        if (repathRoutine != null)
            StopCoroutine(repathRoutine);

        repathRoutine = StartCoroutine(RepathNextFrame());
    }

    IEnumerator RepathNextFrame()
    {
        // Breaks FindNewPath <-> NavigatePath recursion.
        yield return null;

        repathQueued = false;
        repathRoutine = null;

        FindNewPath();
    }

    // ==========================
    // Path selection
    // ==========================
    public void FindNewPath()
    {
        if (!canMove) return;
        if (controller == null || controller.currentlyOccupiedGridnode == null) return;

        GridNode targetNode = null;

        switch (currentMovementBehaviour)
        {
            case NPCMovementBehaviour.Idle:
                currentPathToNavigate = null;
                return;

            case NPCMovementBehaviour.Roam:
                targetNode = GetRoamTarget();
                break;

            case NPCMovementBehaviour.Pursue:
                targetNode = playerGridNode;
                break;
        }

        if (targetNode == null)
        {
            currentPathToNavigate = null;
            return;
        }

        currentPathToNavigate = Pathfinding_Custom.FindPath(controller.currentlyOccupiedGridnode, targetNode);

        // Keep this call if you want immediate responsiveness.
        // It's now safe because NavigatePath no longer calls FindNewPath directly.
        NavigatePath(currentPathToNavigate);
    }

    GridNode GetRoamTarget()
    {
        GridNode fallbackAdjacent = GetRandomAdjacentWalkableNode(previousTargetNode);

        if (roamPlanner == null || vision == null)
            return fallbackAdjacent;

        return roamPlanner.GetRoamTarget(controller.currentlyOccupiedGridnode, fallbackAdjacent, vision);
    }

    GridNode GetRandomAdjacentWalkableNode(GridNode previous = null)
    {
        // NOTE: this call allocates because GetNeighbouringNodes builds a new list.
        // It's fine for now; later we can switch to cached neighbouringNodes.
        List<GridNode> neighbours = new List<GridNode>(controller.currentlyOccupiedGridnode.neighbouringNodes);
        List<GridNode> walkable = new List<GridNode>();

        foreach (GridNode node in neighbours)
        {
            if (node == null) continue;
            if (node.nodeData == null || !node.nodeData.isWalkable) continue;
            if (node.currentOccupant == null) continue;

            var occ = node.currentOccupant.occupantType;
            if (occ == GridNodeOccupantType.None || occ == GridNodeOccupantType.PressurePlate)
                walkable.Add(node);
        }

        if (previous != null && walkable.Contains(previous) && walkable.Count > 1)
            walkable.Remove(previous);

        if (walkable.Count == 0) return null;
        return walkable[Random.Range(0, walkable.Count)];
    }

    // ==========================
    // Navigation / steering
    // ==========================
    public void NavigatePath(List<GridNode> pathToNavigate)
    {
        if (pathToNavigate == null || pathToNavigate.Count == 0)
        {
            if (currentMovementBehaviour != NPCMovementBehaviour.Idle)
                QueueRepath();
            return;
        }

        if (isMoving || isTurning || controller.attackController.isAttacking)
            return;

        GridNode next = pathToNavigate[pathToNavigate.Count - 1];
        currentNavTargetNode = next;

        Vector3 toTarget = next.moveToTransform.position - currentOrientation.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.0001f) return;

        toTarget.Normalize();
        float signed = Vector3.SignedAngle(currentOrientation.forward, toTarget, Vector3.up);

        const float forwardToleranceDeg = 10f;
        bool facing = Mathf.Abs(signed) <= forwardToleranceDeg;

        var occ = next.currentOccupant.occupantType;

        bool stepIsWalkable = (occ == GridNodeOccupantType.None || occ == GridNodeOccupantType.PressurePlate);
        bool stepIsPlayer = (occ == GridNodeOccupantType.Player);

        if (facing)
        {
            if (stepIsWalkable)
            {
                MoveToNode(next);
                return;
            }

            if (stepIsPlayer)
            {
                controller.TryAttack();
                return;
            }

            // Facing but blocked => stale path/target. Repath safely.
            QueueRepath();
            return;
        }

        if (signed > 0f) Turn(1);
        else Turn(-1);
    }

    // ==========================
    // Movement / turning execution
    // ==========================
    void MoveToNode(GridNode nodeToMoveTo)
    {
        if (isMoving) return;

        isMoving = true; // critical: set immediately (prevents re-entrant navigation)

        controller.currentlyOccupiedGridnode.ResetOccupant();
        AnimateMovement();

        AudioClip randClip = HelperFunctions.GetRandomAudioClipFromArray(controller.npcData.walkSFX);
        if (randClip != null)
            controller.audioSource.PlayOneShot(randClip);

        StartCoroutine(LerpPos(transform.position, nodeToMoveTo.moveToTransform.position, controller.npcData.moveDuration));
        StartCoroutine(DelayBetweenMovement());

        previousTargetNode = controller.currentlyOccupiedGridnode;
        controller.currentlyOccupiedGridnode = nodeToMoveTo;
        controller.currentlyOccupiedGridnode.SetOccupant(new GridNodeOccupant(controller.gameObject, GridNodeOccupantType.NPC));
    }

    void Turn(int turnDir)
    {
        if (isTurning) return;

        AnimateTurning(turnDir);
        StartCoroutine(DelayBetweenTurning());
    }

    void AnimateMovement() => controller.animController.PlayAnimation("Walk");

    void AnimateTurning(int turnDir)
    {
        isTurning = true;

        if (turnDir == -1)
            controller.animController.PlayAnimation("TurnLeft", controller.npcData.turnDuration);
        else if (turnDir == 1)
            controller.animController.PlayAnimation("TurnRight", controller.npcData.turnDuration);

        UpdateLookDir(turnDir);
    }

    void UpdateLookDir(int turnDir)
    {
        currentOrientation.Rotate(new Vector3(0, turnDir * 90, 0));
    }

    IEnumerator LerpPos(Vector3 startPos, Vector3 endPos, float lerpDuration)
    {
        float timeElapsed = 0;

        while (timeElapsed < lerpDuration)
        {
            float t = timeElapsed / lerpDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        isMoving = false;
    }

    IEnumerator DelayBetweenMovement()
    {
        yield return new WaitForSeconds(controller.npcData.moveDuration);
        controller.animController.PlayAnimation("Idle");
        yield return new WaitForSeconds(controller.npcData.minDelayBetweenMovement);
        MovementEnded();
    }

    IEnumerator DelayBetweenTurning()
    {
        yield return new WaitForSeconds(controller.npcData.turnDuration + controller.npcData.minDelayBetweenTurning);
        TurningEnded();
    }

    void MovementEnded()
    {
        if (currentMovementBehaviour == NPCMovementBehaviour.Roam && roamPlanner != null)
            roamPlanner.ConsumeTurn();

        EvaluateDetectionAndMaybeAggro();

        controller.TryAttack();
        NavigatePath(currentPathToNavigate);
    }

    void TurningEnded()
    {
        isTurning = false;

        if (currentMovementBehaviour == NPCMovementBehaviour.Roam && roamPlanner != null)
            roamPlanner.ConsumeTurn();

        EvaluateDetectionAndMaybeAggro();

        controller.TryAttack();
        NavigatePath(currentPathToNavigate);
    }

    // ==========================
    // Utility (existing hooks)
    // ==========================
    public void SnapToNode(GridNode node)
    {
        if (!isMoving) return;
        transform.position = node.moveToTransform.position;
        isMoving = false;
    }

    public void SnapToRotation(float newRot)
    {
        transform.Rotate(new Vector3(0, newRot, 0));
        currentOrientation.Rotate(new Vector3(0, newRot, 0));
    }
}
