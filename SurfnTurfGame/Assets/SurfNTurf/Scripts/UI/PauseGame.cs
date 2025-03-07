using UnityEngine;

public class PauseGame : MonoBehaviour
{
	bool paused = false;

	public GameObject pauseScreen;

	public Animator pauseAnimator;

	public GameObject creditScreen;
	public UIManager uiManager;

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape)) togglePause();
	}

	private void togglePause()
	{
		if (paused)
		{
			Unpause();
			paused = false;
		}
		else
		{

			Pause();
			paused = true;
		}

		uiManager.LoadVsync();
	}

	public void Pause()
    {
		if(!pauseScreen.activeInHierarchy) pauseScreen.SetActive(true);
		else pauseAnimator.Play("Pause");
		Time.timeScale = 0f;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	public void Unpause()
	{
		pauseAnimator.Play("Unpause");
		creditScreen.SetActive(false);
		Time.timeScale = 1f;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}
}
