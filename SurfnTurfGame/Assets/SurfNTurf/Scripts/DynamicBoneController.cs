using UnityEngine;

public class DynamicBoneController : MonoBehaviour
{
    [SerializeField] private DynamicBone TailBone;
    public void SetTailBone(int Int)
    {
        TailBone.enabled = Int == 1;
    }
}
