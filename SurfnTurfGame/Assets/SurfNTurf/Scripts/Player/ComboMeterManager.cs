using UnityEngine;

public class ComboMeterManager : MonoBehaviour
{
    //this script takes the values from the combo meter and applies it to the UI
    public float comboCoolDown;

	private void Update()
	{
		ComboMeter.OnUpdate();
	}
}
