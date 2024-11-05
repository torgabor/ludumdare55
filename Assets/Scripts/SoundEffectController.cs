using UnityEngine;

public class SoundEffectController : MonoBehaviour
{
    public AudioSyncSettings[] m_settings;
    public float m_previousAudioValue;
    public float m_audioValue;
    
    public AudioPlayerSync player;
    public Vector3 beatWeights;
    public Vector3 beatTime;

    public Vector3 displacement;

    protected virtual void Start()
    {
        
    }

    protected virtual void Update()
    {
        // update audio value
        m_previousAudioValue = m_audioValue;
        var currentTime = Time.time;
        m_audioValue = player.GetSpectrumData();
        
        ProcessBeatTiming(currentTime);
        displacement = ProcessDisplacement(currentTime);        
    }

    protected void ProcessBeatTiming(float currentTime)
    {
        for (var i = 0; i < m_settings.Length; i++)
        {
            var setting = m_settings[i];
            var nextValidActivationTime = setting.timeStep + beatTime[i] + setting.totalTime;
            
            if ((m_previousAudioValue > setting.bias && m_audioValue <= setting.bias) ||
                (m_previousAudioValue <= setting.bias && m_audioValue > setting.bias))
            {
                if (currentTime > nextValidActivationTime)
                    beatTime[i] = Time.time;
            }
        }
    }

    protected Vector3 ProcessDisplacement(float currentTime)
    {
        Vector3 displacement = Vector3.zero;
        for (var i = 0; i < m_settings.Length; i++)
        {
            var setting = m_settings[i];
            var timeElapsed = currentTime - beatTime[i];
            if (timeElapsed > setting.totalTime)
            {
                displacement[i] = 0;
                continue;
            }
            
            float t = timeElapsed / setting.totalTime;
            var val = setting.curve.Evaluate(t);
            displacement[i] = val * beatWeights[i];
        }
        return displacement;
    }
} 