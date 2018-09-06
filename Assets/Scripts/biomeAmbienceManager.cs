using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

//Implimenta secondary aesthetic properites of biomes, like post processing
public class biomeAmbienceManager : MonoBehaviour {

	void Start () {
        setAmbience();
	}
	
	public void setAmbience () {
        biomeAmbienceData ambienceData = GetComponent<MapGenerator>().biome.ambienceData;
        Camera.main.backgroundColor = ambienceData.cameraBackgroundColor;
        PostProcessingBehaviour cameraBehaviour = Camera.main.GetComponent<PostProcessingBehaviour>();
        if (cameraBehaviour.profile != ambienceData.postProcessingProfile)
        {
            cameraBehaviour.profile = ambienceData.postProcessingProfile;
        }
        FindObjectOfType<Light>().color = ambienceData.directionalLightColor;
        FindObjectOfType<Light>().intensity = ambienceData.directionalLightIntensity;
    }
}
