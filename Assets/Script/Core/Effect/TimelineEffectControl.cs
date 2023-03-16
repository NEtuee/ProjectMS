using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System;

public class TimelineEffectControl : MonoBehaviour//, INotificationReceiver
{
    public PlayableDirector         _playableDirector;
    public bool                     _isCharacterMaterialEffect = false;

    public void setCharacterAnimator(Animator characterAnimator)
    {
        TimelineAsset timelineAsset = (TimelineAsset)_playableDirector.playableAsset;
        TrackAsset track = timelineAsset.GetOutputTrack(1) ;
        _playableDirector.SetGenericBinding (track, characterAnimator);
    }
}