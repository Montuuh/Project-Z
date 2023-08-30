using UnityEngine;

public class ExtraHUD : MonoBehaviour
{
    public void ToggleExtraHUD()
    {
        this.gameObject.SetActive(!this.gameObject.activeSelf);
    }

    public bool IsExtraHUDActive()
    {
        return this.gameObject.activeSelf;
    }
}
