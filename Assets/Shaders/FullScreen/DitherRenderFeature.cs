using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

internal class DitherRendererFeature : ScriptableRendererFeature
{
    public Shader m_Shader;
    public float m_DitherSpread;
    public int m_ColorResolution;

    Material m_Material;

    DitherPass m_RenderPass = null;

    public override void AddRenderPasses(ScriptableRenderer renderer,
                                    ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
            renderer.EnqueuePass(m_RenderPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer,
                                        in RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            // Calling ConfigureInput with the ScriptableRenderPassInput.Color argument
            // ensures that the opaque texture is available to the Render Pass.
            m_RenderPass.ConfigureInput(ScriptableRenderPassInput.Color);
            m_RenderPass.SetTarget(renderer.cameraColorTargetHandle, m_DitherSpread, m_ColorResolution);
        }
    }

    public override void Create()
    {
        m_Material = CoreUtils.CreateEngineMaterial(m_Shader);
        m_RenderPass = new DitherPass(m_Material);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(m_Material);
    }
}