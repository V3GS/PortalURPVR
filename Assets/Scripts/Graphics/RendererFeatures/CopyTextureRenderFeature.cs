using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CopyTextureRenderFeature : ScriptableRendererFeature
{ 
    [System.Serializable]
    public class Settings
    {
        public bool isEnabled = true;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        public string cameraName = "";
        public Shader copyShader;
    }

    private CopyTexturePass m_CopyTexturePass;

    [SerializeField]
    private Settings m_passSettings = new Settings();

    public override void Create()
    {
        m_CopyTexturePass = new CopyTexturePass(m_passSettings, name);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!m_passSettings.isEnabled) return;

        if (m_passSettings.cameraName == "")
        {
            Debug.LogWarningFormat("This pass requires a camera name to be added.");
            return;
        }

        if (!m_passSettings.copyShader)
        {
            Debug.LogWarningFormat("This pass requires a shader to copy the color texture.");
            return;
        }

        if (renderingData.cameraData.camera.name.Equals(m_passSettings.cameraName))
        {
            renderer.EnqueuePass(m_CopyTexturePass);            
        }
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        m_CopyTexturePass.ConfigureInput(ScriptableRenderPassInput.Color);
        
        m_CopyTexturePass.Setup(renderer.cameraColorTargetHandle, renderingData);
    }

    protected override void Dispose(bool disposing)
    {
        m_CopyTexturePass?.CleanUp();
    }
}
