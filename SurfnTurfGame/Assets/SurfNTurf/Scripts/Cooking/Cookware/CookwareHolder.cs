using UnityEngine;

public class CookwareHolder : MonoBehaviour
{
    public GridManager cookware;
    public bool unlocked;
    public bool active;

    public void ShowCookware()
    {
        if (!active)
        {
            active = true;
            gameObject.SetActive(true);
            transform.localScale = Vector3.one;
        }
    }

    public void HideCookware()
    {
        if (active)
        {
            active = false;
            gameObject.SetActive(false);
            transform.localScale = Vector3.zero;
        }
    }

    public void UnlockCookware(float _cellScale)
    {
        cookware.ActivateGrid(_cellScale);
        unlocked = true;
    }
}
