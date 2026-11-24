using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitController : MonoBehaviour
{
    private GameObject selectedGameObject;
    private NavMeshAgent agent;
    // public UnitHitController unitHitController; //get e set
    public UnitAttackController unitAttackController;

    private void Awake()
    {
        // unitHitController = GetComponent<UnitHitController>();
        unitAttackController = GetComponent<UnitAttackController>();

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        selectedGameObject = transform.Find("Selected").gameObject;
        SetSelectedVisible(false);
    }

    void Update()
    {
        if (unitAttackController.CurrentTarget != null && unitAttackController.canAttack == false)
        {
            SetDestination(unitAttackController.CurrentTarget.transform.position);
        }
    }

    /**
    *   Toggle "selected" visual effect under unit.
    */
    public void SetSelectedVisible(bool visible)
    {
        selectedGameObject.SetActive(visible);
    }

    /**
    * 
    */
    public void SetDestination(Vector2 pos)
    {
        agent.SetDestination(pos);
    }

    public void SetAttackTarget(UnitController attackTarget)
    {
        unitAttackController.CurrentTarget = attackTarget;
        if (attackTarget != null)
        {
            SetDestination(attackTarget.transform.position);
        }
    }
}
