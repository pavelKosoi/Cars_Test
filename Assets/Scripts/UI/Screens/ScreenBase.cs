using UnityEngine;

public abstract class ScreenBase : MonoBehaviour, ISwitchable
{  
    public void Hide()
    {
        gameObject.SetActive(false);      
    }

    public void Show()
    {
        gameObject.SetActive(true);        
    }
}
