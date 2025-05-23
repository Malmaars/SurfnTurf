using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class DollyCartPositionAsset : PlayableAsset, ITimelineClipAsset
{
    public DollyCartPositionBehaviour template = new DollyCartPositionBehaviour();

    public ClipCaps clipCaps => ClipCaps.Blending;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<DollyCartPositionBehaviour>.Create(graph, template);
        return playable;
    }
}