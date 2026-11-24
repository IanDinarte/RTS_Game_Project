using UnityEngine;

/**
 * Fazer barra de vida aparecer em cima do boneco quando ele sofre dano.
 *  
 */
public class UnitHitController : MonoBehaviour
{
    [SerializeField] private int maxHealth;
    [SerializeField] private int currentHealth;
    [SerializeField] private int armor;

    private void Update()
    {
        // if (currentHealth <= 0)
        // {
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

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}