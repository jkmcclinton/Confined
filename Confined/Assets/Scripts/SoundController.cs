using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour {

    AudioSource src;
    float pitch;

	// Use this for initialization
	void Start () {
        src = GetComponent<AudioSource>();
        pitch = src.pitch;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void playSound(string sound, bool rnd = true, float r = .2f) {
        string file = "Sounds/" + sound;
        AudioClip clip = Resources.Load(file) as AudioClip;
        if (clip != null) {
            src.pitch = rnd?Vars.clamp(pitch + Random.Range(-r, r),
                -3, 3):pitch;
            src.PlayOneShot(clip);
        } else { Debug.Log("\"" + file + "\" is not\" found"); }
    }
}
