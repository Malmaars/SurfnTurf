using UnityEngine;
//scriptable object to hold input containers
[CreateAssetMenu(fileName = "InputContainers", menuName = "Scriptable Objects/InputContainers", order = 1)]
public class InputContainers : ScriptableObject
{
    public InputContainer[] inputContainers;
}

