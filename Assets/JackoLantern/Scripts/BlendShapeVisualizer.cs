﻿using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARKit;

[RequireComponent(typeof(ARFace))]

public class BlendShapeVisualizer : MonoBehaviour
{
    [SerializeField]
    private BlendShapeMappings blendShapeMappings;

    [SerializeField]
    private SkinnedMeshRenderer skinnedMeshRenderer;

    private ARKitFaceSubsystem arKitFaceSubsystem;

    private Dictionary<ARKitBlendShapeLocation, int> faceArkitBlendShapeIndexMap = new Dictionary<ARKitBlendShapeLocation, int>();

    private ARFace face;

    private void Awake()
    {
        face = GetComponent<ARFace>();
        CreateFeatureBlendMapping();
    }

    private void CreateFeatureBlendMapping()
    {
        if (skinnedMeshRenderer == null || skinnedMeshRenderer.sharedMesh == null)
        {
            return;
        }

        if (blendShapeMappings.Mappings == null)
        {
            Debug.LogError("Mappings must be configured before using blendshapevisualizer");
            return;
        }

        foreach (Mapping mapping in blendShapeMappings.Mappings)
        {
            faceArkitBlendShapeIndexMap[mapping.location] = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(mapping.name);
        }
    }

    void SetVisible(bool visible)
    {
        if (skinnedMeshRenderer == null) return;

        skinnedMeshRenderer.enabled = visible;
    }

    void UpdateVisibility()
    {
        var visible = enabled && (face.trackingState == TrackingState.Tracking) && (ARSession.state > ARSessionState.Ready);

        SetVisible(visible);
    }

    void OnEnable()
    {

        var faceManager = FindObjectOfType<ARFaceManager>();
        if (faceManager != null)
        {
            arKitFaceSubsystem = (ARKitFaceSubsystem)faceManager.subsystem;
        }
        UpdateVisibility();
        face.updated += OnUpdated;
        ARSession.stateChanged += OnSystemStateChanged;
    }

    void OnSystemStateChanged(ARSessionStateChangedEventArgs eventArgs)
    {
        UpdateVisibility();
    }

    void OnUpdated(ARFaceUpdatedEventArgs eventArgs)
    {
        UpdateVisibility();
        UpdateFaceFeatures();
    }

    void UpdateFaceFeatures()
    {
        if (skinnedMeshRenderer == null || !skinnedMeshRenderer.enabled || skinnedMeshRenderer.sharedMesh == null)
        {
            return;
        }

        using (var blendShapes = arKitFaceSubsystem.GetBlendShapeCoefficients(face.trackableId, Allocator.Temp))
        {
            foreach (var featureCoefficient in blendShapes)
            {
                int mappedBlendShapeIndex;
                if (faceArkitBlendShapeIndexMap.TryGetValue(featureCoefficient.blendShapeLocation, out mappedBlendShapeIndex))
                {
                    if (mappedBlendShapeIndex >= 0)
                    {
                        skinnedMeshRenderer.SetBlendShapeWeight(mappedBlendShapeIndex, featureCoefficient.coefficient * blendShapeMappings.CoefficientScale);
                    }
                }
            }
        }

    }

}