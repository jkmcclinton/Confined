using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour {

    float maxVol;
    AudioSource src;
    SceneController sc;
    public AudioClip nextSong;
    public bool changingSong;
    public AudioClip song { set {
            if(src.clip!=null)
                if (value.name.Equals(src.clip.name)) return;
            nextSong = value; changingSong = true; }
    }

	// Use this for initialization
	void Start () {
        sc = FindObjectOfType<SceneController>();
        src = GetComponent<AudioSource>();
        maxVol = src.volume;
	}
	
	// Update is called once per frame
	void Update () {

        if (sc.state == SceneController.StateType.FADE_IN) {
            src.volume = Mathf.Lerp(0, maxVol, sc.fade / sc.fadeTime);
        }  if (sc.state == SceneController.StateType.FADE_OUT) {
            src.volume = Mathf.Lerp(maxVol,0, sc.fade / sc.fadeTime);
        }

        if (changingSong) {
            if (src.clip == null) {
                begin();
            } else {
                // fade between songs ONLY when the current song nears end of current loop
                src.volume = Mathf.Lerp(src.volume, 0, .5f);

                // if finished:
                if(src.volume<.01f){
                    src.volume = maxVol;
                    begin();
                }
            }

        }
	}

    void begin() {
        changingSong = false;
        src.clip = nextSong;
        nextSong = null;
        src.Play();
    }
}
