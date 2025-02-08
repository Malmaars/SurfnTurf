using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class PlayerTeleporter : MonoBehaviour
{
    private Transform teleportPosition;
    private Animator transitioner;

    public UnityEvent onTransition;

    private void Start()
    {
        transitioner = GameObject.Find("TransitioningCanvas").GetComponent<Animator>();
        teleportPosition = transform.GetChild(0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 3)
        {
            StartCoroutine(PerformTeleport(other.gameObject.transform));
        }
    }

    IEnumerator PerformTeleport(Transform _PlayerBody)
    {
        MovementController playerController = _PlayerBody.parent.GetComponent<MovementController>();

        //Lock player movement and play transition fadeIn animation
        playerController.enabled = false;

        transitioner.Play("TransitionFadeIn");
        yield return new WaitForSeconds(transitioner.GetCurrentAnimatorStateInfo(0).length);

        //Teleport object and invoke optional events
        _PlayerBody.GetComponent<TrailRenderer>().enabled = false;
        _PlayerBody.position = teleportPosition.position;

        onTransition.Invoke();

        //Play transition fadeOut animation and unlock player movement
        transitioner.Play("TransitionFadeOut");

        playerController.enabled = true;
        _PlayerBody.GetComponent<TrailRenderer>().enabled = true;

        yield return null;
    }
}
