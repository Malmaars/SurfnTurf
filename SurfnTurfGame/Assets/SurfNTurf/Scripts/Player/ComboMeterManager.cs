using System.Collections;
using TMPro;
using UnityEngine;

public class ComboMeterManager : MonoBehaviour
{
    //this script takes the values from the combo meter and applies it to the UI
    public float comboCoolDown;

    public GameObject comboUI;

    public TextMeshProUGUI comboMeterUI;
    public TextMeshProUGUI comboNameUI;

    bool UIActive;

    private void Awake()
    {
        ComboMeter.comboCoolDown = comboCoolDown;
        
        ComboMeter.onAddCombo +=  SetComboMeterUI;
        ComboMeter.onAddCombo +=  SetComboNameUI;

        TurnOffUI();
    }

    private void Update()
	{
		ComboMeter.OnUpdate();

        if (UIActive && ComboMeter.comboCoolDownTimer <= 0)
            TurnOffUI();

        else if (!UIActive && ComboMeter.comboCoolDownTimer > 0)
            TurnOnUI();
	}

    void TurnOnUI()
    {
        //turn on the ui with a fade or an animation
        UIActive = true;
        comboUI.SetActive(true);
    }
    void TurnOffUI()
    {
        UIActive = false;
        comboUI.SetActive(false);
    }

    void SetComboMeterUI()
    {
        comboMeterUI.text = "X" + ComboMeter.currentCombo.ToString();
    }

    void SetComboNameUI()
    {
        comboNameUI.text = ComboMeter.lastMove;
    }


}
