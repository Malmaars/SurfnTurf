using UnityEngine;

public class URLOpener : MonoBehaviour
{
    public void OpenURL(string _URLName)
    {
        Application.OpenURL(_URLName);
    }
}
