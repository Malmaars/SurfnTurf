using UnityEngine;
using System.Collections.Generic;

public class ServingManager : MonoBehaviour
{
    public static ServingManager instance;

    public bool isActive;
    public bool presentingDish;
    public DishGrid dishGrid;
    public LayerMask layers;
    public GameObject currentPhysicalButton;
    public NPC currentNPC;

    public PlateHolder[] plates;

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
        Debug.Log("Open Serving Menu");
        instance.gameObject.SetActive(true);
        currentNPC = _currentNPC;
        isActive = true;
    }

    public void CloseServingMenu()
    {
        isActive = false;
    }

    public void HandlePhysicalUI()
    {
        if (CollidingWithPhysicalButton())
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
    private bool CollidingWithPhysicalButton()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, layers))
        {
            if (hit.transform.tag == "PhysicalButton")
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
                return true;
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
        return false;
    }
    public void PresentDish(int selectedPlate)
    {
        if (plates[selectedPlate].mainCells.Count == 0)
            return;
        if (currentNPC.GiveDish(plates[selectedPlate]))
        {
            dishGrid.ActivateGrid(0);
            ExtractPlate(plates[selectedPlate]);
        }
        else
        {

        }
    }

    public void ExtractPlate(PlateHolder selectedPlate)
    {
        List<FoodCell> cells = new();

        cells.AddRange(selectedPlate.GetCells());
        selectedPlate.ExtractDish();

        dishGrid.SetCells(cells, dishGrid.gridCenter);
    }
}
