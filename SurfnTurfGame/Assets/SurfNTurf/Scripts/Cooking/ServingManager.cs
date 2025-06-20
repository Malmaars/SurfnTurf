using UnityEngine;
using System.Collections.Generic;

public class ServingManager : MonoBehaviour
{
    public static ServingManager instance;

    public Camera servingCamera;
    public bool isActive;
    public bool presentingDish;
    public DishGrid dishGrid;
    public LayerMask layers;
    public GameObject currentPhysicalButton;
    public NPC currentNPC;

    public PlateHolder[] plates;
    public GameObject[] plateVisuals;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            instance.gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (isActive)
        {
            HandlePhysicalUI();
        }
    }

    public void OpenServingMenu(NPC _currentNPC)
    {
        isActive = true;
        instance.gameObject.SetActive(true);
        currentNPC = _currentNPC;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        CheckPlateVisuals();
    }

    public void CloseServingMenu()
    {
        isActive = false;
        currentNPC = null;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        dishGrid.ClearGrid();
        instance.gameObject.SetActive(false);

    }

    public void CheckPlateVisuals()
    {
        for (int i = 0; i < plates.Length; i++)
        {
            if (plates[i].mainCells.Count != 0)
                plateVisuals[i].SetActive(true);
            else
                plateVisuals[i].SetActive(false);
        }
    }

    public void HandlePhysicalUI()
    {
        CollidingWithPhysicalButton();
        if(currentPhysicalButton != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                currentPhysicalButton.GetComponent<PhysicalButton>().OnMouseDown.Invoke();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                currentPhysicalButton.GetComponent<PhysicalButton>().OnMouseUp.Invoke();
            }
        }
    }
    private void CollidingWithPhysicalButton()
    {
        Ray ray = servingCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, layers))
        {
            if (hit.transform.CompareTag("PhysicalButton"))
            {
                if (currentPhysicalButton != hit.transform.gameObject)
                {
                    if (currentPhysicalButton != null)
                    {
                        currentPhysicalButton.GetComponent<PhysicalButton>().OnMouseExit.Invoke();
                        currentPhysicalButton = null;
                    }
                    currentPhysicalButton = hit.transform.gameObject;
                    currentPhysicalButton.GetComponent<PhysicalButton>().OnMouseEnter.Invoke();
                }
            }
        }
        else
        {
            if (currentPhysicalButton != null)
            {
                currentPhysicalButton.GetComponent<PhysicalButton>().OnMouseExit.Invoke();
                currentPhysicalButton = null;
            }
        }
    }
    public void PresentDish(int selectedPlate)
    {
        if (plates[selectedPlate].mainCells.Count == 0)
            return;
        /*
        if (currentNPC.GiveDish())
        {
            dishGrid.ActivateGrid(0);
            ExtractPlate(plates[selectedPlate], true);
        }
        else
        {
            dishGrid.ActivateGrid(0);
            ExtractPlate(plates[selectedPlate], false);
        }
        */
    }

    public void ExtractPlate(PlateHolder selectedPlate, bool clearDish)
    {
        List<FoodCell> cells = new();
        cells = selectedPlate.GetCells();
        foreach (FoodCell cell in cells)
        {
            Vector2Int gridPos = cell.gridPosition + dishGrid.gridCenter;
            dishGrid.GenerateCellOnGrid(gridPos.x, gridPos.y, cell.cellID, cell.cellTexturePosition, cell.textureGridSize, cell.originalIngredient);
        }
        foreach (FoodCell cell in dishGrid.cells)
        {
            cell.altered = true;
            cell.UpdateVisual();
        }
        if (clearDish)
            selectedPlate.ClearDish();
    }
}
