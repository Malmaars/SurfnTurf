using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore;

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
    private TMP_Text TMPGui;

    public override void Initialize()
    {
        base.Initialize();
        animator.SetBool("isOpen", isOpen);
        ItemHolder.SetActive(false); // Initially hide the item holder
        for (int i = 0; i < SurfBoardManager.instance.surfBoards.Length; i++)
        {
            GameObject item = Instantiate(SurfBoardManager.instance.surfBoards[i]); // Instantiate the item prefab
            item.transform.SetParent(ItemHolder.transform); // Set the parent to the ItemHolder
            item.transform.localPosition = Vector3.zero + new Vector3(i,0,0); // Reset local position
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
                    TMPGui.text = shopItems[i].GetComponent<SurfBoard>().surfBoardName + "\n" + shopItems[i].GetComponent<SurfBoard>().price.ToString() + " Coins";  // Set highlight on the item
                }

            }
            for (int i = 0; i < shopItems.Count; i++)
            {
                shopItems[i].transform.Rotate(Vector3.forward * 20 * Time.deltaTime); // Rotate each item
                if (shopItems[i] == hightLightedItem) // Check if the item is highlighted
                {
                    highlightItemPosition = i; // Update highlight item position
                    shopItems[i].transform.localScale = Vector3.Lerp(shopItems[i].transform.localScale, Vector3.one *0.3f, 0.1f); // Scale up the highlighted item
                }
                else // If the item is adjacent to the highlighted item
                {
                    shopItems[i].transform.localScale = Vector3.Lerp(shopItems[i].transform.localScale, Vector3.one *(0.3f*(((math.distance(highlightItemPosition, i)/shopItems.Count)*-1)+1)), 0.1f); // Scale down other items
                }
            }

            ItemHolder.transform.localPosition = Vector3.Lerp(ItemHolder.transform.localPosition,
             new Vector3(-highlightItemPosition, ItemHolder.transform.localPosition.y, ItemHolder.transform.localPosition.z), 0.1f); // Update item holder position
        }

        // Handle input for item selection and navigation
        if (Input.GetKeyDown(KeyCode.LeftArrow))
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
        else if (Input.GetKeyDown(KeyCode.RightArrow))
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
    }

    private void OpenShop()
    {
        isOpen = true;
        animator.SetBool("isOpen", isOpen);
        ItemHolder.SetActive(true); // Show the item holder when the shop is open
        // Additional logic to open the shop UI can be added here
    }

    private void CloseShop()
    {
        isOpen = false;
        animator.SetBool("isOpen", isOpen);
        ItemHolder.SetActive(false); // Hide the item holder when the shop is closed
        TMPGui.text = "Shop";
        // Additional logic to close the shop UI can be added here
    }
}
