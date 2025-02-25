using UnityEngine;

public class PlayerState : MonoBehaviour
{ 

	public virtual void ExitState()
	{
		enabled = false;
	}

	public virtual void EnterState()
	{

	}
}
