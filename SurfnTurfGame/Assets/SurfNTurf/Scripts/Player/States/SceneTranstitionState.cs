using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SceneTranstitionState : PlayerState
{
    [SerializeField] private Image fadeImage;
    private float fadeDuration = 0.2f;
    [HideInInspector] public GameObject[] CurrentSceneObjects;
    [HideInInspector] public GameObject[] NextSceneObjects;
    [HideInInspector] public Transform playerSpawnPoint;

    [HideInInspector] public GameObject player;

    //singleton instance
    public static SceneTranstitionState instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public override void EnterState()
    {
        fadeImage.color = new Color(0, 0, 0, 0);
        StartCoroutine(TransitionCoroutine());
    }

    public override void ExitState()
    {
        fadeImage.color = new Color(0, 0, 0, 0);
        base.ExitState();
    }

    public override void InitStateTransitions()
    {
        player = BlackBoard.playerBody.gameObject;
        base.InitStateTransitions();
    }


    private IEnumerator TransitionCoroutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        EnableObjects();
        WaitForSeconds wait = new WaitForSeconds(0.3f);
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1 - (elapsedTime / fadeDuration));
            fadeImage.color = new Color(0, 0, 0, alpha);
            player.transform.position = playerSpawnPoint.position;
            player.transform.rotation = playerSpawnPoint.rotation;
            yield return null;
        }
        Debug.Log("Transition complete!");
        BlackBoard.playerManager.SwitchState(typeof(MovementController));

    }

    private void EnableObjects()
    {
        foreach (GameObject obj in CurrentSceneObjects)
        {
            obj.SetActive(false);
        }
        foreach (GameObject obj in NextSceneObjects)
        {
            obj.SetActive(true);
        }
    }

}
