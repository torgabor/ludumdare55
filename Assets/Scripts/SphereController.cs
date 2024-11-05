using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SoundEffectController))]
public class SphereController : MonoBehaviour
{
    public Vector3 rotation;
    

    private Renderer m_renderer;
    private MaterialPropertyBlock materialBlock;

    public SoundEffectController soundEffectController;    

    public void Awake(){
        soundEffectController = GetComponent<SoundEffectController>();
        m_renderer = GetComponent<Renderer>();
        materialBlock = new MaterialPropertyBlock();
        m_renderer.GetPropertyBlock(materialBlock);    
    }
 

    public void LateUpdate()
    {
        materialBlock.SetVector("_Displace", soundEffectController.displacement);
        m_renderer.SetPropertyBlock(materialBlock);

        //Rotate the sphere
        var newRot = transform.rotation.eulerAngles + rotation * Time.deltaTime;
        transform.rotation = Quaternion.Euler(newRot);
        
    }
    
}