using UnityEngine;
using UnityEngine.AI;

/**
 *  
 */
public class UnitHitController : MonoBehaviour
{
    private UnitAttackController unitAttackController;
    private UnitController unitController;
    public HealthBar healthBar;
    private NavMeshAgent agent;
    private Animator animator;
    [SerializeField] private int maxHealth;
    [SerializeField] private int currentHealth;
    [SerializeField] private int armor;
    public int MaxHealth { get => maxHealth; set => maxHealth = value; }
    public int CurrentHealth { get => currentHealth; set => currentHealth = value; }
    private bool isDead;
    
    public Vector2[] attackSlots;

    private void Awake()
    {
        unitAttackController = GetComponent<UnitAttackController>();
        unitController = GetComponent<UnitController>();
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        healthBar = GetComponentInChildren<HealthBar>();
        healthBar.SetVisible(false);

        isDead = false;
    }

    private void Update()
    {
        if (CurrentHealth > maxHealth)
        {
            CurrentHealth = maxHealth;
        }

        if (CurrentHealth <= 0 && !isDead)
        {
            CurrentHealth = 0;
            Die();
        }
    }

    public void TakeDamage(int damage)
    {
        float mitigation = 1f;

        if (armor > 100)
        {
            int diference = armor - 100;

            mitigation = (float)((armor - diference) * 0.6f) / 100;

            mitigation += (float)(diference * 0.3f) / 100;
        }
        else
        {
            mitigation = (float)(armor * 0.6f) / 100;
        }

        if (mitigation > 0.9f)
        {
            mitigation = 0.9f;
        }

        damage -= (int)(damage * mitigation);

        CurrentHealth -= damage;
        healthBar.UpdateHealthBar(CurrentHealth, maxHealth);
        healthBar.SetVisible(true);

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        gameObject.tag = "Dead";
        animator.SetBool("Dead", true);
        isDead = true;

        if (unitController != null)
        {
            unitController.CurrentBehaviour = UnitController.UnitBehaviour.PASSIVE;
            if (GameRTSController.Instance != null)
            {
                GameRTSController.Instance.DeselectUnit(unitController);
            }
        }

        healthBar.SetVisible(false);
        unitAttackController.CurrentTarget = null;
        unitAttackController.CurrentTargetHitController = null;
        agent.enabled = false;
    }
}