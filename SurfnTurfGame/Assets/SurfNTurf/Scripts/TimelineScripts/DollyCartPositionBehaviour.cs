using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Unity.Cinemachine;

[System.Serializable]
public class DollyCartPositionBehaviour : PlayableBehaviour
{
    public AnimationCurve positionCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private CinemachineSplineDolly resolvedCart;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        var cart = playerData as CinemachineSplineDolly;
        if (cart == null) return;

        double time = playable.GetTime();
        float t = Mathf.Clamp01((float)(time / playable.GetDuration()));
        cart.CameraPosition = positionCurve.Evaluate(t);
    }
}
