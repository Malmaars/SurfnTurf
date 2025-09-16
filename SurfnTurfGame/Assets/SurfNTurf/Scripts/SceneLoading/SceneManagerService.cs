using Eflatun.SceneReference;
using UnityEngine;

public class SceneManagerService : MonoBehaviour
{
    public static SceneManagerService Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void LoadScene(SceneReference scene)
    {
        if (IsSceneLoaded(scene)) return;
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene.Name, UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }

    public void UnloadScene(SceneReference scene)
    {
        if (!IsSceneLoaded(scene)) return;
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene.Name);
    }
    
    public bool IsSceneLoaded(SceneReference scene)
    {
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            var loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (loadedScene.name == scene.Name)
            {
                return true;
            }
        }
        return false;
    }
}
