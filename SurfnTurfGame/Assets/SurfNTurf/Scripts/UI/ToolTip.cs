using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class ToolTip : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public static ToolTip instance { get; private set; }
    [SerializeField] private GameObject toolTipPanel;
    [SerializeField] private Image toolTipIcon;
    [SerializeField] private TextMeshProUGUI toolTipTitle;
    [SerializeField] private TextMeshProUGUI toolTipDescription;
    private Vector3 targetPosition;
    public bool isActive = false;
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
    private void Update()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(targetPosition);
        toolTipPanel.transform.position = screenPos;  
    }
    public void OnHoverEnter(ToolTipData toolTipData, Vector3 position)
    {
        targetPosition = position;
        isActive = true;
        //set the tooltip data
        toolTipTitle.text = toolTipData.title;
        toolTipDescription.text = toolTipData.description;
        toolTipIcon.sprite = toolTipData.icon;
        animator.SetBool("isActive", true);
    }
    public void OnHoverExit()
    {
        if (!isActive)
            return;
        isActive = false;
        animator.SetBool("isActive", false);
    }
}
[System.Serializable]
public class ToolTipData
{
    public string title = "Title";
    public string description = "Description";
    public Sprite icon;

}
