using System.Collections;
using UnityEngine;

public class SurfBoardManager : MonoBehaviour
{
    [SerializeField] private GameObject surfBoardSocket;
    public GameObject[] surfBoards;
    [SerializeField] private GameObject decal; // Prefabs for the surfboards
    public static SurfBoardManager instance { get; private set; }
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
    IEnumerator Start()
    {
        yield return new WaitUntil(() => CloudSaveSystem.Instance != null && CloudSaveSystem.Instance.IsInitialized);
        for (int i = 0; i < surfBoards.Length; i++)
        {
            GameObject surfBoard = Instantiate(surfBoards[i], surfBoardSocket.transform.position, surfBoardSocket.transform.rotation, surfBoardSocket.transform);
            SurfBoard surfboardComponent = surfBoards[i].GetComponent<SurfBoard>();
            surfBoard.transform.localScale = new Vector3(0.005f,0.005f,0.005f); // Set local position to zero
            surfBoards[i] = surfBoard; // Store the instantiated board in the array
            if (i == CloudSaveSystem.Instance.data.surfboardEquipped)
            {
                surfboardComponent.isEquipped = true; // Set the equipped surfboard
            }
            else
            {
                surfboardComponent.isEquipped = false; // Set the equipped surfboard
            }

            surfBoard.SetActive(false); // Deactivate the board after instantiation
        }
        yield break; 
    }

    public void ChangeSurfBoard(int index)
    {
        if (index < 0 || index >= surfBoards.Length) return; // Check for valid index

        // Deactivate the current surfboard
        if (CloudSaveSystem.Instance.data.surfboardEquipped >= 0 && CloudSaveSystem.Instance.data.surfboardEquipped < surfBoards.Length)
        {
            surfBoards[CloudSaveSystem.Instance.data.surfboardEquipped].SetActive(false);
        }

        // Activate the new surfboard
        CloudSaveSystem.Instance.data.surfboardEquipped = index;
        surfBoards[CloudSaveSystem.Instance.data.surfboardEquipped].SetActive(true);
    }

    public void ToggleSurfboard(bool isActive)
    {
        surfBoards[CloudSaveSystem.Instance.data.surfboardEquipped].SetActive(isActive);
        decal.SetActive(!isActive);
    }

}

