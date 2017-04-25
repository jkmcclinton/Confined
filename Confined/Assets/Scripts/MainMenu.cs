using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour {

    GameObject highlight, outlinedObj;
    SceneController sc;
    Camera cam;
    Material outline;
    SoundController sound;
    float t;

	// Use this for initialization
	void Start () {
        sc = FindObjectOfType<SceneController>();
        cam = FindObjectOfType<Camera>();
        outline = Resources.Load("Materials/Outline")
            as Material;
        sound = FindObjectOfType<SoundController>();
    }
	
	// Update is called once per frame
	void Update () {
        if (sc.state == SceneController.StateType.BLANK) {
            //load next scene;
            Camera.main.backgroundColor = Color.black;
            Destroy(GameObject.Find("SceneObj"));
            SceneManager.LoadScene("Main");
        }

        //handle input
        if (sc.state != SceneController.StateType.NORMAL) return;

        if (highlight == null) {
            highlight = AutoDisable.Find("Wake_Up");
            highlight.SetActive(true);
        } else t += Time.deltaTime;

        if(outlinedObj == null && t > 1.5f) { 
            Mesh mesh = highlight.GetComponentInChildren<MeshFilter>().mesh;
            outlinedObj = new GameObject("Outlined Obj");
            outlinedObj.transform.parent = transform;
            outlinedObj.transform.SetPositionAndRotation(highlight.transform.position, 
                highlight.transform.rotation);
            MeshRenderer mR = outlinedObj.AddComponent<MeshRenderer>();
            MeshFilter mF = outlinedObj.AddComponent<MeshFilter>();
            mF.mesh = mesh;
            mR.material = outline;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Talk")) {
            if (highlight != null) {
                sc.fadeOut();
                sound.playSound("Start", false);
                sc.fadeTime = 5;
            }
        }
	}
}
