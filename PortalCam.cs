using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
傳送們特效的視覺同步與渲染，不包含觸發移動
放置對象: 這個腳本必須掛載到輔助攝影機 (Portal Camera / Camera B) 上。
通常，您會將這個輔助攝影機放在 AnchorPoint 物件（即傳送門 B）的子物件中，或者與其位置和旋轉同步。
重要: 確保這個物件上只有一個 Camera 組件（腳本已透過 [RequireComponent(typeof(Camera))] 強制要求）。
*/

[RequireComponent(typeof(Camera))]
public class PortalCam : MonoBehaviour
{
    [Header("玩家的攝影機")]
    public Transform playerCamera;

    [Header("玩家前方的傳送門 A")]
    public Transform portal_FrontPlayer;

    [Header("照準點 / 傳送門 B")]
    public Transform AnchorPoint;

    [Header("動態產生新Portal材質")]
    public Material cameraMatB;

    [Header("RenderTexture")]
    public int rtWidth = 1920;
    public int rtHeight = 1080;
    public int rtDepth = 24;

    [Header("選項")]
    public bool enableMirrorZ = false; // 是否對 Z 軸做鏡像（典型 portal 會需要）
    public bool invertYRotation = false; // 若畫面上下顛倒，可嘗試開啟

    private Camera camB;
    private RenderTexture rt;

    void Start()
    {
        camB = GetComponent<Camera>();
        CreateRenderTexture();
    }

    void CreateRenderTexture()
    {
        if (camB == null) return;

        if (camB.targetTexture != null)
        {
            camB.targetTexture.Release();
        }

        if (rt != null)
        {
            if (rt.IsCreated()) rt.Release();
            Destroy(rt);
        }

        rt = new RenderTexture(rtWidth, rtHeight, rtDepth);
        rt.Create();
        camB.targetTexture = rt;

        if (cameraMatB != null)
            cameraMatB.mainTexture = rt;
    }

    void LateUpdate()
    {
        if (playerCamera == null || portal_FrontPlayer == null || AnchorPoint == null) return;

        // 使用 TransformPoint / InverseTransformPoint 更直觀也會處理 scale
        Vector3 localPos = portal_FrontPlayer.InverseTransformPoint(playerCamera.position);

        if (enableMirrorZ)
        {
            localPos.z = -localPos.z; // 前後鏡像
        }

        transform.position = AnchorPoint.TransformPoint(localPos);

        // 旋轉映射
        Quaternion localRot = Quaternion.Inverse(portal_FrontPlayer.rotation) * playerCamera.rotation;

        if (enableMirrorZ)
        {
            // 若做鏡像，通常需要額外翻轉（180 around up 或 X 視情況）
            // 這裡示範繞 Y 軸 180°
            localRot = Quaternion.Euler(0f, 180f, 0f) * localRot;
        }

        transform.rotation = AnchorPoint.rotation * localRot;

        if (invertYRotation)
        {
            transform.rotation = transform.rotation * Quaternion.Euler(180f, 0f, 0f);
        }
    }

    void OnDisable()
    {
        CleanUpRT();
    }

    void OnDestroy()
    {
        CleanUpRT();
    }

    void CleanUpRT()
    {
        if (camB != null) camB.targetTexture = null;
        if (rt != null)
        {
            if (rt.IsCreated()) rt.Release();
            Destroy(rt);
            rt = null;
        }
    }
}
