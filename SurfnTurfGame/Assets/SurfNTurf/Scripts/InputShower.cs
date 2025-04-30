using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputShower : MonoBehaviour
{
    [Expandable] public InputContainers inputContainers;

    public GameObject inputUIContainer;
    public GameObject inputUIContainerPrefab;
    GameObject newInputUIContainer;
    public float collomTimer = 0.0f;

    private void Start()
    {
        // enable all input actions in the input containers
        foreach (var inputContainer in inputContainers.inputContainers)
        {
            inputContainer.inputAction.Enable();
        }
    }

    private void Update()
    {
        foreach (var inputContainer in inputContainers.inputContainers)
        {
            if (inputContainer.inputAction.triggered && inputContainer.inputAction.WasPressedThisFrame())
            {

                //Debug.Log(inputContainer.name + " was pressed");

                inputContainer.inputAction.Enable();
                // create a new input container object and set its text to the name of the input container
                if (collomTimer > 20f || newInputUIContainer == null || newInputUIContainer.transform.childCount == 9)
                {
                    // destroy the old input UI container after 10 seconds
                    newInputUIContainer = Instantiate(inputUIContainerPrefab, inputUIContainer.transform);
                    newInputUIContainer.transform.SetSiblingIndex(0);
                    newInputUIContainer.SetActive(true);
                    Destroy(newInputUIContainer, 3f);
                    collomTimer = 0.0f;
                }
                GameObject InputUI = Instantiate(newInputUIContainer.transform.GetChild(0).gameObject, newInputUIContainer.transform);
                //set texture on the image component of the child object
                InputUI.GetComponent<RawImage>().texture = inputContainer.icon;
                //set input ui to child position 1
                InputUI.transform.SetSiblingIndex(1);
                InputUI.SetActive(true);
            }
            collomTimer += Time.deltaTime;
        }
    }

}

[System.Serializable]
public class InputContainer
{
    public string name;
    public InputAction inputAction;
    [ShowAssetPreview] public Texture2D icon;
}