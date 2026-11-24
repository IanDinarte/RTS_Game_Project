using UnityEngine;
using UnityEngine.AI;

public class UnitAttackController : MonoBehaviour
{
    private NavMeshAgent agent;
    private UnitController currentTarget;
    public UnitController CurrentTarget { get => currentTarget; set => currentTarget = value; }
    private UnitHitController currentTargetHitController;

    public int attackDamage;
    public float attackRange;
    public float attackCooldown;
    private float lastAttackTime;
    public bool canAttack;


    void Awake()
    {
    }

    void Update()
    {
        if (CurrentTarget != null)
        {
            if (currentTargetHitController == null)
            {
                currentTargetHitController = CurrentTarget.GetComponent<UnitHitController>();
            }
            if (Vector2.Distance(this.transform.position, CurrentTarget.transform.position) <= attackRange)
            {
                agent.SetDestination(this.transform.position);
                canAttack = true;

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    Attack();
                }
            }
            else
            {
                canAttack = false;
            }
        }
        else
        {
            currentTargetHitController = null;
            canAttack = false;
        }
    }

    private void Attack()
    {
        //change animation sprite
        lastAttackTime = Time.time;
        currentTargetHitController.TakeDamage(attackDamage);
    }
}