﻿using UnityEngine;
using System.Collections;

public class CameraGizmos : MonoBehaviour
{
    public float nearClipPlane = 0.3f;
    public float farClipPlane = 1000.0f;

    public virtual void OnDrawGizmos()
    {
        Matrix4x4 temp = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        if (Camera.main.orthographic)
        {
            float spread = farClipPlane - nearClipPlane;
            float center = (farClipPlane + nearClipPlane) * 0.5f;
            Gizmos.DrawWireCube(new Vector3(0, 0, center), new Vector3(Camera.main.orthographicSize * 2 * Camera.main.aspect, Camera.main.orthographicSize * 2, spread));
        }
        else
        {
            Gizmos.DrawFrustum(Vector3.zero, Camera.main.fieldOfView, farClipPlane, nearClipPlane, Camera.main.aspect);
        }
        Gizmos.matrix = temp;
    }
}