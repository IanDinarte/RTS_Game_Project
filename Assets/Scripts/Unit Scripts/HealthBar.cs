using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private Transform fillTransform;

    void Awake()
    {
        fillTransform = transform.Find("Fill");
    }

    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (fillTransform == null)
        {
            fillTransform = transform.Find("Fill");
        }

        float result = (float)currentHealth / maxHealth;

        fillTransform.localScale = new Vector3(result, 1f, 1f);
        // Debug.Log(result);
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
