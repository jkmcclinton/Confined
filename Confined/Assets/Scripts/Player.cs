using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Player : MonoBehaviour {
    
    CharacterController cc;
    Rigidbody rb;
    Vector3 moveDir = Vector3.zero;
    Camera cam;
    Animator anim;
    /*[HideInInspector] */public Interactable currentInteractable;
    AudioSource src;
    SceneController sc;
    GameObject bubble;
    Dialogue dialogue;
    SoundController sound;
    GameObject console;
    public Animator animator { get { return anim; } }
    //public Warp door;

    public float pitch;
    public float speed = 1;
    public float MaxTurnSpeed = 180;

    // Use this for initialization
    void Start () {
        sc = FindObjectOfType<SceneController>();
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        cam = Camera.main;
        src = GetComponent<AudioSource>();
        pitch = src.pitch;
        bubble = transform.FindChild("Interact").gameObject;
        dialogue = FindObjectOfType<Dialogue>();
        console = GameObject.Find("Console");
        console.SetActive(false);
        sc.setConsole(console);
        sound = FindObjectOfType<SoundController>();
    }
	
	// Update is called once per frame
	void Update () {
        if (currentInteractable != null) {
            bubble.SetActive(!currentInteractable.door);
        } else bubble.SetActive(false);
	}

    public void interact() { anim.SetTrigger("Interact"); }
    public void yawn() { anim.SetTrigger("Yawn"); playSound("yawn"); }
    
    private void FixedUpdate() {

        if (!console.activeSelf)  {
            if (Input.GetButtonDown("Console")) {
                anim.SetBool("Moving", false);
                console.SetActive(true);
                InputField iF = console.GetComponentInChildren<InputField>();
                iF.text = "";
                iF.Select();
            }
            if (Input.GetButtonDown("Quit")) {
                Application.Quit();
                return;
            }

            // movement and input stuff
            if (sc.state != SceneController.StateType.NORMAL) return;
            if (!sc.analyzing) {
                Vector2 input = handleInput();

                // always move along the camera forward as it is the direction that it being aimed at
                Vector3 desiredMove = new Vector3(input.x, 0, input.y);
                if (desiredMove.magnitude > 0) {
                    anim.SetBool("Moving", true);
                    Quaternion want = Quaternion.LookRotation(desiredMove);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, want, MaxTurnSpeed * Time.deltaTime);
                } else {
                    anim.SetBool("Moving", false);
                }

                // get a normal for the surface that is being touched to move along it
                RaycastHit hitInfo;
                Physics.SphereCast(transform.position, cc.radius, Vector3.down, out hitInfo,
                                   cc.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
                desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

                moveDir.x = desiredMove.x * speed;
                moveDir.z = desiredMove.z * speed;

                moveDir.y = -10;
                cc.Move(moveDir * Time.fixedDeltaTime);
            } else {
                anim.SetBool("Moving", false);
                // if has speech,
                string page = dialogue.getPage();
                if (dialogue.stateType != Dialogue.StateType.HIDDEN &&
                    dialogue.stateType != Dialogue.StateType.HIDING) {
                    if (Input.GetButtonDown("Talk")) {
                        if (dialogue.stateType == Dialogue.StateType.SHOWN)
                            if (dialogue.si < page.Length/* && dialogue.skipCoolDown <= 0*/) {
                                dialogue.fill();
                            } else /*if (dialogue.si >= page.Length && dialogue.skipCoolDown <= 0)*/ {
                                dialogue.skipCoolDown = .5f;
                                dialogue.hidePage();
                                //sound.playSound("OK", false);
                                sc.analyze();
                            }
                    }
                } else
                    sc.analyze();
            }
        } else {
            if (Input.GetButtonDown("Quit")) {
                console.SetActive(false);
            }
        }
    }

    public void playSound(string sound, bool rnd = true) {
        string file = "Sounds/" + sound;
        AudioClip clip = Resources.Load(file) as AudioClip;
        if (clip != null) {
            float r = .2f;
            src.pitch = rnd ? Vars.clamp(pitch + Random.Range(-r, r),
                -3, 3) : pitch;
            src.PlayOneShot(clip);
        } else { Debug.Log("\"" + file + "\" is not\" found"); }
    }

    void playStepSound() {
        playSound("Step_" + sc.groundType.ToString());
    }

    private Vector3 handleInput() {
        if (Input.GetButton("Interact") && currentInteractable!=null/* && door==null*/) {
            // start dialogue system
            AudioClip clip = Resources.Load("Sounds/Open") as AudioClip;
                src.PlayOneShot(clip);
            if (currentInteractable.react) interact();
            sc.loadScript(currentInteractable.scriptFile);
            currentInteractable.mark();
            currentInteractable = null;
            return Vector3.zero;
        }
        
        return new Vector2(Input.GetAxis("Horizontal"), 
            Input.GetAxis("Vertical")) * speed;
    }
}
