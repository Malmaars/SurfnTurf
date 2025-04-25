using UnityEngine;

public class SurfBoardManager : MonoBehaviour
{
    [SerializeField] private GameObject surfBoardSocket;
    public GameObject[] surfBoards;
    [SerializeField] private SavedProperty<int> currentSurfBoard; // Prefabs for the surfboards
    [SerializeField] private GameObject decal; // Prefabs for the surfboards
    public static SurfBoardManager instance { get; private set; }
    private void Awake()
    {
        currentSurfBoard = new (nameof(currentSurfBoard) + this, currentSurfBoard.Value);
        
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        for (int i = 0; i < surfBoards.Length; i++)
        {
            GameObject surfBoard = Instantiate(surfBoards[i], surfBoardSocket.transform.position, surfBoardSocket.transform.rotation, surfBoardSocket.transform);
            surfBoard.transform.localScale = new Vector3(0.005f,0.005f,0.005f); // Set local position to zero
            surfBoards[i] = surfBoard; // Store the instantiated board in the array
            surfBoard.SetActive(false); // Deactivate the board after instantiation
        }
    }

    public void ChangeSurfBoard(int index)
    {
        if (index < 0 || index >= surfBoards.Length) return; // Check for valid index

        // Deactivate the current surfboard
        if (currentSurfBoard >= 0 && currentSurfBoard < surfBoards.Length)
        {
            surfBoards[currentSurfBoard].SetActive(false);
        }

        // Activate the new surfboard
        currentSurfBoard.Value = index;
        surfBoards[currentSurfBoard].SetActive(true);
    }

    public void ToggleSurfboard(bool isActive)
    {
        surfBoards[currentSurfBoard].SetActive(isActive);
        decal.SetActive(!isActive);
    }

}

