using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class RenderTexturesManager
{
    public static RTHandle TemporaryColorTexture;
}

public class CopyTexturePass : ScriptableRenderPass
{
    private string m_ProfilerTag;
    private readonly Material m_copyMaterial;

    private RTHandle m_CameraColorTarget;

    public CopyTexturePass(CopyTextureRenderFeature.Settings settings, string tag)
    {
        this.renderPassEvent = settings.renderPassEvent;
        m_ProfilerTag = tag;
        m_copyMaterial = new Material(settings.copyShader);
    }

    public void Setup(RTHandle colorHandle, in RenderingData renderingData)
    {
        m_CameraColorTarget = colorHandle;

        var colorCopyDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        colorCopyDescriptor.depthBufferBits = (int)DepthBits.None;

        RenderingUtils.ReAllocateIfNeeded(ref RenderTexturesManager.TemporaryColorTexture, colorCopyDescriptor, FilterMode.Point, TextureWrapMode.Clamp, name: "_TemporaryColorTexture");
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ConfigureTarget(m_CameraColorTarget);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {        
        if (renderingData.cameraData.camera.cameraType != CameraType.Game || m_copyMaterial == null)
        {
            return;
        }

        CommandBuffer cmd = CommandBufferPool.Get();

        using (new ProfilingScope(cmd, new ProfilingSampler(m_ProfilerTag)))
        {
            cmd.SetGlobalTexture("_TextureToCopy", m_CameraColorTarget.nameID);

            Blitter.BlitCameraTexture(cmd, m_CameraColorTarget, RenderTexturesManager.TemporaryColorTexture, m_copyMaterial, 0);
        }

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        CommandBufferPool.Release(cmd);
    }

    public void CleanUp()
    {
        CoreUtils.Destroy(m_copyMaterial);
    }
}
