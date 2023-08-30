using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class GrabTexturePass : ScriptableRenderPass
{
    string m_ProfilerTag;
    private readonly Material m_effectMaterial;

    private RTHandle m_CameraColorTarget;

    private int m_cameraColorCopyId = Shader.PropertyToID("_CameraColorCopyTexture");

    public GrabTexturePass(GrabTextureRenderFeature.Settings settings, string tag)
    {
        this.renderPassEvent = settings.renderPassEvent;
        m_effectMaterial = settings.portalMaterial;

        m_ProfilerTag = tag;
    }

    public void Setup(RTHandle colorHandle, in RenderingData renderingData)
    {
        m_CameraColorTarget = colorHandle;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ConfigureTarget(m_CameraColorTarget);
        cmd.GetTemporaryRT(m_cameraColorCopyId, renderingData.cameraData.cameraTargetDescriptor);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {        
        if (renderingData.cameraData.camera.cameraType != CameraType.Game || m_effectMaterial == null)
        {
            return;
        }        

        CommandBuffer cmd = CommandBufferPool.Get();

        using (new ProfilingScope(cmd, new ProfilingSampler(m_ProfilerTag)))
        {
            cmd.SetGlobalTexture("_CameraColorCopy", RenderTexturesManager.TemporaryColorTexture);

            Blitter.BlitCameraTexture(cmd, RenderTexturesManager.TemporaryColorTexture, m_CameraColorTarget, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, m_effectMaterial, 0);
        }

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        CommandBufferPool.Release(cmd);
    }

    public void CleanUp()
    {
        RTHandles.Release(RenderTexturesManager.TemporaryColorTexture);
    }
}
