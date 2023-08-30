using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
public class GrabTextureRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public bool isEnabled = true;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        public string cameraName = "";

        public Material portalMaterial = null;
    }

    private GrabTexturePass m_GrabPass;
    [SerializeField]
    private Settings m_passSettings = new Settings();

    public override void Create()
    {            
        m_GrabPass = new GrabTexturePass(m_passSettings, name);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!m_passSettings.isEnabled) return;

        if (m_passSettings.portalMaterial == null)
        {
            Debug.LogWarningFormat("Missing portal Material. {0} won't be executed. Check for missing reference in the assigned renderer.", GetType().Name);
            return;
        }

        if (m_passSettings.cameraName == "")
        {
            Debug.LogWarningFormat("This pass requires a camera name to be added.");
            return;
        }

        if (renderingData.cameraData.camera.name.Equals(m_passSettings.cameraName))
        {                    
            renderer.EnqueuePass(m_GrabPass);            
        }
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        m_GrabPass.ConfigureInput(ScriptableRenderPassInput.Color);
        m_GrabPass.Setup(renderer.cameraColorTargetHandle, renderingData);
    }

    protected override void Dispose(bool disposing)
    {
        m_GrabPass?.CleanUp();
    }
}
