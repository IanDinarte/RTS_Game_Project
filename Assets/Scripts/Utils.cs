
using UnityEngine;
using UnityEngine.InputSystem;

public static class Utils
{

    public static Vector3 GetWorldMousePosition()
    {
        if (Mouse.current == null || Camera.main == null)
            return Vector3.zero;

        Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
        return Camera.main.ScreenToWorldPoint(mouseScreenPosition);
    }
}