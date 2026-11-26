using UnityEngine;
using UnityEngine.AI;

/**
 * Animação espelhada para andar pro outro lado.
*/
[RequireComponent(typeof(NavMeshAgent))]
public class UnitController : MonoBehaviour
{
    private GameObject selectedGameObject;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private NavMeshAgent agent;
    // public UnitHitController unitHitController; //get e set
    public UnitAttackController unitAttackController;

    /**
     * How the units will behave when provoked (i.e attacked or aggroed).
     * AGGRESSIVE: Will follow the target until it's dead.
     * DEFENSIVE: Will follow the target to a certain point then go back.
     * STAND: Won't follow the target but will attack if it's in range.
     * PASSIVE: Won't react at all.
    */
    private enum Behaviour
    {
        AGGRESSIVE,
        DEFENSIVE,
        STAND,
        PASSIVE
    }
    [SerializeField] private Behaviour currentBehaviour = Behaviour.AGGRESSIVE;
    private Behaviour CurrentBehaviour { get => currentBehaviour; set => currentBehaviour = value; }

    [SerializeField] private float aggroRange;
    public bool isMoving;
    public bool isManualMove;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        unitAttackController = GetComponent<UnitAttackController>();
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        // unitHitController = GetComponent<UnitHitController>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        selectedGameObject = transform.Find("Selected").gameObject;
        SetSelectedVisible(false);
    }

    void Update()
    {
        isMoving = agent.pathPending || agent.velocity.sqrMagnitude > 0.0f;
        animator.SetBool("IsMoving", isMoving);

        if (isManualMove && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            isManualMove = false;
        }

        if (unitAttackController != null)
        {
            if (unitAttackController.CurrentTarget != null && !unitAttackController.inAttackRange)
            {
                SetDestination(unitAttackController.CurrentTarget.transform.position);
            }
            else if (unitAttackController.CurrentTarget == null && !isManualMove && !isMoving)
            {
                AutoBehave();
            }
        }

        UpdateFacingDirection();
    }

    /**
    *   Toggle "selected" visual effect under unit.
    */
    public void SetSelectedVisible(bool visible)
    {
        selectedGameObject.SetActive(visible);
    }

    public void OrderMove(Vector2 pos)
    {
        SetAttackTarget(null);
        isManualMove = true;
        SetDestination(pos);
    }

    private void SetDestination(Vector2 pos)
    {
        // animator.SetBool("IsMoving",true);
        agent.SetDestination(pos);
    }

    public void StopAction()
    {
        isManualMove = false;
        agent.ResetPath();
        // agent.isStopped = true;
        SetAttackTarget(null);
    }

    public void SetAttackTarget(UnitController attackTarget)
    {
        unitAttackController.CurrentTarget = attackTarget;

        if (attackTarget != null)
        {
            isManualMove = false;
            SetDestination(attackTarget.transform.position);
            unitAttackController.CurrentTargetHitController = attackTarget.GetComponent<UnitHitController>();
        }
    }

    private void UpdateFacingDirection()
    {
        if (isMoving)
        {
            if (Mathf.Abs(agent.velocity.x) > 0.01f)
            {
                spriteRenderer.flipX = agent.velocity.x < 0f;
            }
        }
        else if (unitAttackController != null && unitAttackController.CurrentTarget != null && unitAttackController.inAttackRange == true)
        {
            float dirToTargetX = unitAttackController.CurrentTarget.transform.position.x - transform.position.x;
            if (Mathf.Abs(dirToTargetX) > 0.01f)
            {
                spriteRenderer.flipX = dirToTargetX < 0f;
            }
        }
    }

    private void AutoBehave()
    {
        switch (currentBehaviour)
        {
            case Behaviour.AGGRESSIVE:
                SetAttackTarget(GetEnemyInRange());
                break;
            case Behaviour.DEFENSIVE:
                break;
            case Behaviour.STAND:
                break;
            default: //Behaviour.PASSIVE
                break;
        }
    }

    private readonly Collider2D[] hitBuffer = new Collider2D[32];
    private UnitController GetEnemyInRange()
    {
        int count = Physics2D.OverlapCircle(transform.position, aggroRange, ContactFilter2D.noFilter, hitBuffer);

        for (int i = 0; i < count; i++)
        {
            if (gameObject.CompareTag("Enemy"))
            {
                if (hitBuffer[i].CompareTag("Ally") || hitBuffer[i].CompareTag("Player"))
                {
                    return hitBuffer[i].GetComponent<UnitController>();
                }
            }
            else
            {
                if (hitBuffer[i].CompareTag("Enemy"))
                {
                    return hitBuffer[i].GetComponent<UnitController>();
                }
            }
        }

        return null;
    }
}
