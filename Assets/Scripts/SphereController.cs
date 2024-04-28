using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class SphereController : MonoBehaviour
{
    public AudioSyncSettings[] m_settings;

    private float m_previousAudioValue;
    private float m_audioValue;


    public Vector3 rotation;
    public AudioClip targetClip;
    private AudioPlayerSync player;

    private Renderer m_renderer;
    private MaterialPropertyBlock materialBlock;
    public Vector3 beatWeights;
    private Vector3 beatTime;

 
    public void Start()
    {
        m_renderer = GetComponent<Renderer>();
        materialBlock = new MaterialPropertyBlock();
        m_renderer.GetPropertyBlock(materialBlock);
        player = AudioManager.Instance.GetTrack(targetClip);
    }

    public void Update()
    {
        //// update audio value
        m_previousAudioValue = m_audioValue;
        var currentTime = Time.time;
        // get the data
        m_audioValue = player.GetSpectrumData();
        
        for (var i = 0; i < m_settings.Length; i++)
        {
            var setting = m_settings[i];
            var nextValidActivationTime = setting.timeStep + beatTime[i] + setting.totalTime;
            // if audio value went below the bias during this frame
            if (m_previousAudioValue > setting.bias &&
                m_audioValue <= setting.bias)
            {
                // if minimum beat interval is reached
                
                if (currentTime > nextValidActivationTime)
                    beatTime[i] = Time.time;
            }

            // if audio value went above the bias during this frame
            if (m_previousAudioValue <= setting.bias &&
                m_audioValue > setting.bias)
            {
                // if minimum beat interval is reached
                if (currentTime > nextValidActivationTime)
                    beatTime[i] = Time.time;
            }
        }

        
        
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
            else
            {
                float t = timeElapsed / setting.totalTime;
                var val = setting.curve.Evaluate(t);
                displacement[i] = val * beatWeights[i];
            }
        }
        
        materialBlock.SetVector("_Displace", displacement);
        m_renderer.SetPropertyBlock(materialBlock);

        //Rotate the sphere
        var newRot = transform.rotation.eulerAngles + rotation * Time.deltaTime;
        transform.rotation = Quaternion.Euler(newRot);
        
    }
    
}