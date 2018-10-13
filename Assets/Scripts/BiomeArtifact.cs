using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Artifact", menuName = "Perlin Maps/Artifact", order = 2)]
public class BiomeArtifact : ScriptableObject {

    public GameObject gameObject;

    [Range(0, 1)] public float perlinValueLowerBound;
    [Range(0, 1)] public float perlinValueUpperBound;
}
