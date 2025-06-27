using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Unity.VisualScripting;

public class PlayerTeleporter : MonoBehaviour
{
    private Transform teleportPosition;
    //public Animator transitioner;

    public UnityEvent onTransition;

    private void Start()
    {
        teleportPosition = transform.GetChild(0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            StartCoroutine(PerformTeleport());
        }
    }

    IEnumerator PerformTeleport()
    {

        //Lock player movement and play transition fadeIn animation
        BlackBoard.playerManager.SwitchState(typeof(CutsceneState));

        //transitioner.Play("TransitionFadeIn");
        //yield return new WaitForSeconds(transitioner.GetCurrentAnimatorStateInfo(0).length);
        yield return new WaitForSeconds(1f);

        //Teleport object and invoke optional events
        BlackBoard.playerBody.position = teleportPosition.position;

        onTransition.Invoke();

        //Play transition fadeOut animation and unlock player movement
        //transitioner.Play("TransitionFadeOut");

        BlackBoard.playerManager.SwitchState(typeof(MovementController));

        yield return null;
    }
}
