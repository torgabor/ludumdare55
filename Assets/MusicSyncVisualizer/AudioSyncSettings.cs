using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Audio/Sync settings")]
public class AudioSyncSettings : ScriptableObject
{
    public float bias = 25;
    [FormerlySerializedAs("animation")] public AnimationCurve curve;
     public float timeStep = 0.1f;
     public float totalTime = 0.3f;
    // public float timeToBeat = 0.05f;
    // public float restSmoothTime = 5f;
}