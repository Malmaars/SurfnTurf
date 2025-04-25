using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class Shop : Interactible
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private GameObject ItemHolder;
    private bool isOpen = false;
    [SerializeField]
    private CinemachineCamera shopCamera;
    [SerializeField]
    private List<GameObject> shopItems = new List<GameObject>();
    private GameObject hightLightedItem;
    [SerializeField]
    private AnimationCurve bounchCurve;


    [SerializeField]
    private GameObject canvas;
    [SerializeField]
    private TMP_Text priceText;
    [SerializeField]
    private TMP_Text nameText;
    [SerializeField]
    private Toggle EquippedToggle;
    [SerializeField]
    private Toggle OwnedToggle;

    public override void Initialize()
    {
        base.Initialize();
        animator.SetBool("isOpen", isOpen);
        canvas.SetActive(false); // Initially hide the canvas
        ItemHolder.SetActive(false); // Initially hide the item holder
        for (int i = 0; i < SurfBoardManager.instance.surfBoards.Length; i++)
        {
            GameObject item = Instantiate(SurfBoardManager.instance.surfBoards[i]); // Instantiate the item prefab
            item.transform.SetParent(ItemHolder.transform); // Set the parent to the ItemHolder
            item.transform.localPosition = Vector3.zero + new Vector3(i, 0, 0); // Reset local position
            item.SetActive(true); // Activate the item
            //face the item upwards
            item.transform.rotation = Quaternion.Euler(-90, 0, 0);
            item.transform.localScale = Vector3.one * 0.2f; // Set local scale
            shopItems.Add(item); // Update the list with the instantiated item
        }
        hightLightedItem = shopItems[0]; // Set the first item as highlighted
    }

    public override bool InteractWith()
    {
        if (isOpen)
        {
            CloseShop();
            return false;
        }
        else
        {
            BlackBoard.cameraController.SwitchToCamera(shopCamera, 0.2f);
            OpenShop();
        }
        return true;
    }
    void Update()
    {
        if (isOpen)
        {
            int highlightItemPosition = 0; // Reset highlight item position
            for (int i = 0; i < shopItems.Count; i++)
            {
                if (shopItems[i] == hightLightedItem) // Check if the item is highlighted
                {
                    highlightItemPosition = i; // Update highlight item position
                    SetText(shopItems[i]);  // Set highlight on the item
                }

            }
            for (int i = 0; i < shopItems.Count; i++)
            {
                shopItems[i].transform.Rotate(Vector3.forward * 20 * Time.deltaTime); // Rotate each item
                if (shopItems[i] == hightLightedItem) // Check if the item is highlighted
                {
                    highlightItemPosition = i; // Update highlight item position
                    shopItems[i].transform.localScale = Vector3.Lerp(shopItems[i].transform.localScale, Vector3.one * 0.3f, 0.1f); // Scale up the highlighted item
                }
                else // If the item is adjacent to the highlighted item
                {
                    shopItems[i].transform.localScale = Vector3.Lerp(shopItems[i].transform.localScale, Vector3.one * (0.3f * (((math.distance(highlightItemPosition, i) / shopItems.Count) * -1) + 1)), 0.1f); // Scale down other items
                }
            }

            ItemHolder.transform.localPosition = Vector3.Lerp(ItemHolder.transform.localPosition,
             new Vector3(-highlightItemPosition, ItemHolder.transform.localPosition.y, ItemHolder.transform.localPosition.z), 0.1f); // Update item holder position
        }
    }

    // Handle input for item selection and navigation

    private void OpenShop()
    {
        isOpen = true;
        animator.SetBool("isOpen", isOpen);
        ItemHolder.SetActive(true); // Show the item holder when the shop is open
        canvas.SetActive(true);
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Shop.Next, Next);
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Shop.Previous, Previous);
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Shop.Buy, Buy);
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Shop.Equip, Equip);
        // Additional logic to open the shop UI can be added here
    }

    private void CloseShop()
    {
        isOpen = false;
        animator.SetBool("isOpen", isOpen);
        ItemHolder.SetActive(false); // Hide the item holder when the shop is closed
        canvas.SetActive(false);
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Shop.Next, Next);
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Shop.Previous, Previous);
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Shop.Buy, Buy);
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Shop.Equip, Equip);

        // Additional logic to close the shop UI can be added here
    }

    private void SetText(GameObject item)
    {
        SurfBoard surfBoard = item.GetComponent<SurfBoard>();
        if (surfBoard != null)
        {
            priceText.text = surfBoard.price.ToString(); // Update the price text
            nameText.text = surfBoard.surfBoardName; // Update the name text
            EquippedToggle.isOn = surfBoard.isEquipped; // Update the equipped toggle
            OwnedToggle.isOn = surfBoard.isUnlocked; // Update the owned toggle
        }
    }

    public void Next(InputAction.CallbackContext context)
    {
        int currentIndex = shopItems.IndexOf(hightLightedItem);
        if (currentIndex < shopItems.Count - 1)
        {
            hightLightedItem = shopItems[currentIndex + 1]; // Move right in the list
        }
        else
        {
            hightLightedItem = shopItems[0]; // Wrap around to the first item
        }
    }
    public void Previous(InputAction.CallbackContext context)
    {
        int currentIndex = shopItems.IndexOf(hightLightedItem);
        if (currentIndex > 0)
        {
            hightLightedItem = shopItems[currentIndex - 1]; // Move left in the list
        }
        else
        {
            hightLightedItem = shopItems[shopItems.Count - 1]; // Wrap around to the last item
        }
    }
    public void Buy(InputAction.CallbackContext context)
    {
        SurfBoard surfBoard = hightLightedItem.GetComponent<SurfBoard>();
        if (surfBoard.isUnlocked == true) return; // Check if the surfboard is already unlocked
        if (CoinSpawner.instance.currentCoinCount >= surfBoard.price)
        {
            CoinSpawner.instance.AddCoinToCounter(-surfBoard.price);
            surfBoard.isUnlocked.Value = true; 
            for (int i = 0; i < shopItems.Count; i++)
            {
                if (shopItems[i] != hightLightedItem) // Deactivate other surfboards
                {
                    shopItems[i].GetComponent<SurfBoard>().isEquipped = false;
                }
            }
            surfBoard.isEquipped = true; // Equip the surfboard
            SurfBoardManager.instance.ChangeSurfBoard(shopItems.IndexOf(hightLightedItem)); // Change the surfboard in the manager
            StartCoroutine(BounchItem(hightLightedItem));
        }
    }
    public void Equip(InputAction.CallbackContext context)
    {
        SurfBoard surfBoard = hightLightedItem.GetComponent<SurfBoard>();
        if (surfBoard.isUnlocked == false) return;
        if (surfBoard.isEquipped) return;

        for (int i = 0; i < shopItems.Count; i++)
        {
            if (shopItems[i] != hightLightedItem) // Deactivate other surfboards
            {
                shopItems[i].GetComponent<SurfBoard>().isEquipped = false;
            }
        }
        surfBoard.isEquipped = true; // Equip the surfboard
        SurfBoardManager.instance.ChangeSurfBoard(shopItems.IndexOf(hightLightedItem)); // Change the surfboard in the manager
        StartCoroutine(BounchItem(hightLightedItem));

    }

    private IEnumerator BounchItem(GameObject item)
    {
        Vector3 originalScale = item.transform.localScale;
        Vector3 targetScale = item.transform.localScale * 1.4f; // Scale up the item
        float elapsedTime = 0f;
        float duration = 0.4f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            item.transform.localScale = Vector3.Lerp(originalScale, targetScale, bounchCurve.Evaluate(elapsedTime / duration)); // Lerp the position
            yield return null;
        }
        item.transform.localScale = originalScale; // Reset to original position after bouncing
    }

}
