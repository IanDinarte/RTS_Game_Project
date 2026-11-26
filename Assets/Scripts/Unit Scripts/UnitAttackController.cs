using UnityEngine;
using UnityEngine.AI;

/**
 * Corrigir animação ataque
*/
[RequireComponent(typeof(NavMeshAgent))]
public class UnitAttackController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private UnitController currentTarget;
    private UnitHitController currentTargetHitController;

    public UnitController CurrentTarget { get => currentTarget; set => currentTarget = value; }
    public UnitHitController CurrentTargetHitController { get => currentTargetHitController; set => currentTargetHitController = value; }
    
    [SerializeField] public int attackDamage;
    [SerializeField] public float attackRange;
    public bool inAttackRange;
    [SerializeField] public float attackCooldown;
    private float lastAttackTime;
    private bool isAttacking;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        isAttacking = false;
    }

    void Update()
    {
        if (CurrentTarget != null)
        {
            if (CurrentTargetHitController == null)
            {
                CurrentTargetHitController = CurrentTarget.GetComponent<UnitHitController>();
            }

            if (CurrentTarget.gameObject.CompareTag("Dead"))
            {
                CurrentTarget = null;
                agent.ResetPath();
            }
            else
            {
                if (Vector2.Distance(this.transform.position, CurrentTarget.transform.position) <= attackRange)
                {
                    agent.ResetPath();
                    // agent.isStopped = true;
                    inAttackRange = true;

                    if (Time.time >= lastAttackTime + attackCooldown && isAttacking == false)
                    {
                        Attack();
                    }
                }
                else
                {
                    inAttackRange = false;
                }
            }
        }
        else
        {
            currentTargetHitController = null;
            inAttackRange = false;
        }
    }

    private void Attack()
    {
        isAttacking = true;
        int variant = Random.Range(1, 3);
        animator.SetInteger("Attack", variant);
    }

    public void DealDamage()
    {
        if (currentTargetHitController != null)
        {
            CurrentTargetHitController.TakeDamage(attackDamage);
        }
    }

    public void OnAttackEnd()
    {
        animator.SetInteger("Attack", 0);
        isAttacking = false;
        lastAttackTime = Time.time;
    }
}