using UnityEngine;
using UnityEngine.Timeline;
using Unity.Cinemachine;

[TrackColor(0.2f, 0.8f, 0.2f)]
[TrackClipType(typeof(DollyCartPositionAsset))]
[TrackBindingType(typeof(CinemachineSplineDolly))]
public class DollyCartPositionTrack : TrackAsset
{
}
