using System;
using UnityEngine;

[Serializable]
public class DialoguePart
{
    public Sprite SpeakerIcon = null;
    public Sprite LeftSpeakerIcon = null;
    public Sprite RightSpeakerIcon = null;
    public string Name = "Speaker Name";
    [TextArea(3, 10)]
    public string Text = "Something I am talking about";
    public bool LeftSideSpeaker;
    public bool BothSideSpeaker = false;
    public bool RightSideSpeaker => !LeftSideSpeaker;
}
