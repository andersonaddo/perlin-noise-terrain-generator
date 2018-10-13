using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ArtifactGenerationManager : MonoBehaviour {
    public int LODForArtifacts;
    public static int _LODforArtifacts;

    public Transform artifactSuperParent;
    public static Transform _artifactSuperParent;

    void Awake () {
        _LODforArtifacts = LODForArtifacts;
        _artifactSuperParent = artifactSuperParent;
    }
}
