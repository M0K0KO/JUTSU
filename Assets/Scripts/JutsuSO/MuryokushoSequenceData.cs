using UnityEngine;

[CreateAssetMenu(fileName = "MuryokushoSequenceData", menuName = "MuryokushoSequenceData", order = 1)]
public class MuryokushoSequenceData : ScriptableObject
{
    [Header("Bloom Quad")]
    public float quadBloomDuration = 0.5f;
    public AnimationCurve quadBloomCurve;

    [Header("Skybox Materials")]
    public Material originalSkyboxMaterial;
    public Material spaceSkyboxMaterial;

    [Header("Dissolve Cutoff data")]
    public float dissolveDuration = 0.5f;
    public float minCutoffHeight = -10f;
    public float maxCutoffHeight = 60f;
    public AnimationCurve cutoffCurve;
    
    public float cutOffHeightRange => maxCutoffHeight - minCutoffHeight;

    [Header("Intersection Sphere")]
    public float intersectionDuration = 0.5f;
    public float minIntersectionSphereScale = 0f;
    public float maxIntersectionSphereScale = 95f;
    public AnimationCurve intersectionSphereScaleCurve;
    
    public float intersectionSphereScaleRange => maxIntersectionSphereScale - minIntersectionSphereScale;
}
