using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RenderToMipmap : MonoBehaviour
{
    [Range(0, 3)]
    public int sourceLevel = 0;

    private Shader m_Shader;
    public Shader shader
    {
        get
        {
            if (m_Shader == null)
                m_Shader = Shader.Find("Hidden/Render to Mipmap");

            return m_Shader;
        }
    }

    private Material m_Material;
    public Material material
    {
        get
        {
            if (m_Material == null)
            {
                if (shader == null || !shader.isSupported)
                    return null;

                m_Material = new Material(shader);
            }

            return m_Material;
        }
    }

    private Camera m_Camera;
    public Camera camera_
    {
        get
        {
            if (m_Camera == null)
                m_Camera = GetComponent<Camera>();

            return m_Camera;
        }
    }

    private RenderTexture m_Mipmap;

    private void RenderFullScreenQuad()
    {
        GL.PushMatrix();
        GL.LoadOrtho();

        //Render the full screen quad manually.
        GL.Begin(GL.QUADS);
        GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(0.0f, 0.0f, 0.1f);
        GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, 0.0f, 0.1f);
        GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, 0.1f);
        GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(0.0f, 1.0f, 0.1f);
        GL.End();

        GL.PopMatrix();
    }

    void OnDisable()
    {
        if (m_Mipmap != null)
        {
            RenderTexture.ReleaseTemporary(m_Mipmap);
            m_Mipmap = null;
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (m_Mipmap == null || (m_Mipmap.width != source.width || m_Mipmap.height != source.height))
        {
            if (m_Mipmap)
                RenderTexture.ReleaseTemporary(m_Mipmap);

            m_Mipmap = RenderTexture.GetTemporary(source.width, source.height, 0, source.format, RenderTextureReadWrite.Default);
            m_Mipmap.filterMode = FilterMode.Bilinear;

            m_Mipmap.useMipMap = true;
            m_Mipmap.autoGenerateMips = false;

            m_Mipmap.hideFlags = HideFlags.HideAndDontSave;
        }

        material.SetFloat("_SourceLevel", (float) sourceLevel);

        Graphics.SetRenderTarget(m_Mipmap, 0);
        material.SetPass(0);
        RenderFullScreenQuad();

        Graphics.SetRenderTarget(m_Mipmap, 1);
        material.SetPass(1);
        RenderFullScreenQuad();

        Graphics.SetRenderTarget(m_Mipmap, 2);
        material.SetPass(2);
        RenderFullScreenQuad();

        Graphics.SetRenderTarget(m_Mipmap, 3);
        material.SetPass(3);
        RenderFullScreenQuad();

        Graphics.Blit(m_Mipmap, destination, material, 4);
    }
}
