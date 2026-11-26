using UnityEngine;

public class AttackStateBehaviour : StateMachineBehaviour
{
    [Range(0f, 1f)]
    public float impactTime = 0.8f;
    private bool hasDealtDamage;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hasDealtDamage = false;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!hasDealtDamage && stateInfo.normalizedTime >= impactTime)
        {
            hasDealtDamage = true;
            UnitAttackController attackController = animator.GetComponentInParent<UnitAttackController>();
            if (attackController != null)
            {
                attackController.DealDamage();
            }
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        UnitAttackController attackController = animator.GetComponentInParent<UnitAttackController>();
        if (attackController != null)
        {
            attackController.OnAttackEnd();
        }
    }
}
