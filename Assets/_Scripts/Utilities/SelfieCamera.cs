using System;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class SelfieCamera : MonoBehaviour
{
    public bool FlipHorizontal;
    
    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();

        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    private void OnDestroy()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }
    
    private void OnEndCameraRendering(ScriptableRenderContext arg1, Camera arg2)
    {
        GL.invertCulling = false;
    }

    private void OnBeginCameraRendering(ScriptableRenderContext arg1, Camera arg2)
    {
        _camera.ResetWorldToCameraMatrix();
        _camera.ResetProjectionMatrix();
        var scale = new Vector3(FlipHorizontal ? -1 : 1, 1, 1);
        _camera.projectionMatrix *= Matrix4x4.Scale(scale);
        
        GL.invertCulling = FlipHorizontal;
    }
}