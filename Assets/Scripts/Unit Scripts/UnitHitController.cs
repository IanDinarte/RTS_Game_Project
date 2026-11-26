using UnityEngine;
using UnityEngine.AI;

/**
 *  
 */
public class UnitHitController : MonoBehaviour
{
    // private UnitAttackController unitAttackController;
    public HealthBar healthBar;
    private NavMeshAgent agent;
    private Animator animator;
    [SerializeField] private int maxHealth;
    public int MaxHealth { get => maxHealth; set => maxHealth = value; }
    [SerializeField] private int currentHealth;
    [SerializeField] private int armor;

    private void Awake()
    {
        // unitAttackController = GetComponent<UnitAttackController>();
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        healthBar = GetComponentInChildren<HealthBar>();
        healthBar.SetVisible(false);
    }

    private void Update()
    {
        if(currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if(currentHealth < 0)
        {
            currentHealth = 0;
        }

        // if (currentHealth <= 0)
        // {
        //     healthBar.SetVisible(false);
        //     Die();
        // }
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

        currentHealth -= damage;
        healthBar.UpdateHealthBar(currentHealth, maxHealth);
        healthBar.SetVisible(true);

        if (currentHealth <= 0)
        {
            healthBar.SetVisible(false);
            Die();
        }
    }

    private void Die()
    {
        //play death animation
        // unitAttackController.CurrentTarget = null;
        // unitAttackController.CurrentTargetHitController = null;
        animator.SetBool("Dead", true);
        agent.enabled = false;
        gameObject.tag = "Dead";
        // Destroy(gameObject);
    }
}