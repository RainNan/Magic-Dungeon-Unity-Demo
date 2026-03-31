using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GestureTemplate", menuName = "Gesture/Template")]
public class GestureTemplate : ScriptableObject
{
    [Serializable]
    public class GestureSample
    {
        public Vector2[] points;
    }

    public SkillName templateName;
    public List<GestureSample> samples = new();
}
