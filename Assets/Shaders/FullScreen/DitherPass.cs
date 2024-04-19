using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

internal class DitherPass : ScriptableRenderPass
{
    ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Dither");
    Material m_Material;
    RTHandle m_CameraColorTarget;
    float m_DitherSpread;
    int m_ColorResolution;

    public DitherPass(Material material)
    {
        m_Material = material;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public void SetTarget(RTHandle colorHandle, float spread, int resolution)
    {
        m_CameraColorTarget = colorHandle;
        m_DitherSpread = spread;
        m_ColorResolution = resolution;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ConfigureTarget(m_CameraColorTarget);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cameraData = renderingData.cameraData;
        if (cameraData.camera.cameraType != CameraType.Game)
            return;

        if (m_Material == null)
            return;

        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, m_ProfilingSampler))
        {
            m_Material.SetFloat("_DitherSpread", m_DitherSpread);
            m_Material.SetInt("_ColorResolution", m_ColorResolution);
            Blitter.BlitCameraTexture(cmd, m_CameraColorTarget, m_CameraColorTarget, m_Material, 0);
        }
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        CommandBufferPool.Release(cmd);
    }
}