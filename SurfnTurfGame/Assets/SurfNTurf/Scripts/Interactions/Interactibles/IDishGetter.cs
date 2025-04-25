using UnityEngine;

public interface IDishGetter
{
	//if the bool returns true, remove the dish from the player
	public bool GiveDish(PlateHolder _dish);
}
