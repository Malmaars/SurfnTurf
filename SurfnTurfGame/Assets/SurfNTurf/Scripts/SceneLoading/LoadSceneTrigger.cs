using Eflatun.SceneReference;
using UnityEngine;

public class LoadSceneTrigger : MonoBehaviour
{
    public SceneReference sceneToLoad;
    void OnTriggerEnter(Collider other)
    {
        SceneManagerService.Instance.LoadScene(sceneToLoad);
    }

    void OnTriggerExit(Collider other)
    {
        SceneManagerService.Instance.UnloadScene(sceneToLoad);
    }
}
