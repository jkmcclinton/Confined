
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SceneController : MonoBehaviour {

    // speech related stuff
    private List<string> script;
    private int index = -1;
    public bool analyzing;
    public StateType state = StateType.FADE_IN;

    public float waitTime;

    GameObject c;
    History history;
    Evaluator evaluator;
    RoomIdentifier current;
    Dialogue dialogue;
    SoundController sound;
    Image overlay;
    Player player;
    MusicController mc;
    CameraShakeController csc;

    public RoomIdentifier Room { get { return current; } set { current = value; } }

    public float fadeTime = 1;
    public float fade;

    // Shake Camera stuff
    public float shakeAmount=.1f;//The amount to shake this frame.
    public float shakeDuration=1f;//The duration this frame.

    //Readonly values...
    float shakePercentage;//A percentage (0-1) representing the amount of shake to be applied when setting rotation.

    bool isShaking = false; //Is the coroutine running right now?
    private GameObject cam;
    public bool smooth;//Smooth rotation?
    public float smoothAmount = 5f;//Amount to smooth
    private float curAmnt, curTime;

    public enum StateType {
        BLANK, FADE_IN, NORMAL, FADE_OUT,
    }

    public RoomIdentifier.GroundType groundType {
        get {
            if (current != null) return current.groundType;
            else return RoomIdentifier.GroundType.Stone;
        }
    }

    // Use this for initialization
    void Start() {
        history = FindObjectOfType<History>();
        evaluator = FindObjectOfType<Evaluator>();
        dialogue = FindObjectOfType<Dialogue>();
        sound = FindObjectOfType<SoundController>();
        player = FindObjectOfType<Player>();
        overlay = GameObject.Find("Canvas").GetComponent<Image>();
        mc = FindObjectOfType<MusicController>();
        csc = FindObjectOfType<CameraShakeController>();
        cam = GameObject.Find("Main Camera");
    }

    public void setConsole(GameObject c) { this.c = c; }

    // Update is called once per frame
    void Update() {
        float t = fade / fadeTime;

        //if (!isShaking) StartCoroutine(Shake());

        switch (state) {
            case StateType.FADE_OUT:
                fade += Time.deltaTime;
                if (fade > fadeTime) {
                    state = StateType.BLANK;
                    overlay.color = new Color(0, 0, 0, 0);
                } else
                    overlay.color = new Color(0, 0, 0, Mathf.Lerp(0, 1, t));
                break;
            case StateType.BLANK:
                overlay.color = new Color(0, 0, 0, 0);
                break;
            case StateType.FADE_IN:
                fade += Time.deltaTime;
                if (fade > fadeTime) {
                    state = StateType.NORMAL;
                    overlay.enabled = false;
                } else
                    overlay.color = new Color(0, 0, 0, Mathf.Lerp(1, 0, t));
                break;
            case StateType.NORMAL:
                break;
        }
    }

    public void console(string line) {
        if (line.Contains("(") && line.Contains(")")) {
            string command = line.Substring(0, line.IndexOf("("));
            execute(command, line);
        }
        c.SetActive(false);
    }

    public void analyze() {
        if (waitTime > 0) {
            waitTime -= Time.deltaTime;
            return;
        }

        if (index >= script.Count) {
            analyzing = false;
            return;
        }
        if (state != StateType.NORMAL) return;


        string line = script[index];

        if (line.StartsWith("{")) {
            dialogue.startPage(line.Substring(line.IndexOf("{") + 1, line.IndexOf("}") - 1));
            index++;
        } else {
            //Debug.Log("LINE: " + line);
            if (!line.Contains("(") || !line.Contains(")")) {
                index++;
                if (index >= script.Count) analyzing = false;
            } else {
                string command = line.Substring(0, line.IndexOf("("));
                execute(command, line);

                if (script != null) {
                    index++;
                    if (index >= script.Count) analyzing = false;
                }
            }
        }
    }

    private string[] getArgs(string line) {
        try {
            return line.Substring(line.IndexOf("(") + 1,
                line.LastIndexOf(")") - 1 - line.IndexOf("(")).Split(',');
        } catch (System.Exception e) {
            Debug.Log(line + "\n" + e.StackTrace);
        }
        return null;
    }

    void execute(string command, string line) {
        string[] args = getArgs(line);
        GameObject obj; Interactable iB;
        bool b_val; int i_val; string s_val;
        switch (command.ToLower().Trim()) {
            case "interact":
                player.interact();
                break;
            case "yawn":
                player.yawn();
                break;
            // enable (objNam, bool)
            case "enable":
                obj = GameObject.Find(args[0]);
                if (obj == null) obj = AutoDisable.Find(args[0]);

                if (obj != null) {
                    iB = obj.GetComponent<Interactable>();
                    if (iB != null) {
                        if (Vars.isBool(args[1])) {
                            iB.isEnabled = bool.Parse(args[1]);
                        } else Debug.LogError("\"" + args[1] + "\" is not a bool.");
                    } else {
                        //Debug.Log("\"" + args[0] + "\" is not an interactable.");
                        if (Vars.isBool(args[1])) {
                            obj.SetActive(bool.Parse(args[1]));
                        } else Debug.LogError("\"" + args[1] + "\" is not a bool.");
                    }
                } else Debug.LogError("Could not find \"" + args[0] + "\".");
                break;

            // if (expression, then)
            // if (expression, then, else)
            case "if":
                //Debug.Log("args: " + args.Length);
                b_val = evaluator.evaluate(args[0]);

                if (args.Length == 2) {
                    if (b_val) {
                        s_val = args[1].Substring(0, args[1].IndexOf("("));
                        execute(s_val, args[1]);
                    }
                } else {
                    i_val = b_val ? 1 : 2;
                    s_val = args[i_val].Substring(0, args[i_val].IndexOf("("));
                    execute(s_val, args[i_val]);
                }

                break;

            // declare(varName, startVal)
            case "declare":
                declare(args);
                break;

            // text("text")
            case "text":
                dialogue.startPage(args[0].Substring(args[0].IndexOf("{") + 1, args[0].IndexOf("}") - 1));
                break;

            // shake()
            // shake(amount, dur)
            case "shake":
                //Debug.Log("args: "+args.Length+":"+args);
                    csc.Shake();
                //if (args.Length == 1) {
                //    //ShakeCamera();
                //}else {
                //    if(Vars.isNumeric(args[0]) && Vars.isNumeric(args[1])) {
                //        //ShakeCamera(float.Parse(args[0]), float.Parse(args[1]));

                //    }
                //}
                break;

            // script(objName, scriptName)
            case "script":
                obj = GameObject.Find(args[0]);
                if (obj != null) {
                    iB = obj.GetComponent<Interactable>();
                    if (iB != null) {
                        iB.scriptFile = (args[1]);
                    } else Debug.LogError("\"" + args[0] + "\" is not an interactable.");
                } else Debug.LogError("Could not find \"" + args[0] + "\".");
                break;

            // set (varName, mathExpr)
            case "set":
                i_val = history.var(args[0]);
                if (i_val != int.MinValue) {
                    declare(args);
                }

                if (args[1].Contains("[")) {
                    i_val = int.Parse(evaluator.evaluateExpression(args[1]));
                } else if (Vars.isNumeric(args[1])) {
                    history.setVar(args[0], int.Parse(args[1]));
                } else Debug.LogError("\"" + args[1] + "\" is not numeric.");
                break;

            // sound(soundName)
            case "sound":
                //obj = GameObject.Find(args[0]);
                //if (obj != null) {
                sound.playSound(args[0]);
                //} else Debug.Log("Could not find \"" + args[0] + "\".");
                break;

            // flag (flagName, boolExpr)
            case "flag":
                if (args.Length == 2)
                    if (Vars.isBool(args[1])) {
                        history.setFlag(args[0], bool.Parse(args[1]));
                    } else Debug.LogError("\"" + args[1] + "\" is not a bool.");
                else
                    history.setFlag(args[0], true);
                break;

            // wait( seconds )
            case "wait":
                if (Vars.isNumeric(args[0])) {
                    waitTime = float.Parse(args[0]);
                } else Debug.LogError("\"" + args[0] + "\" is not numeric.");
                break;
        }
    }

    private void declare(string[] args) {
        if (Vars.isNumeric(args[1])) {
            history.setVar(args[0], int.Parse(args[1]));
        } else Debug.LogError("\"" + args[1] + "\" is not numeric.");
    }

    public void loadScript(string scriptKey) {
        if (analyzing) return;
        index = 0;
        analyzing = true;

        string f = Vars.SCRIPT_LIST[scriptKey.Trim()];
        script = new List<string>(File.ReadAllLines(f));
    }

    public void fadeOut() {
        if (state != StateType.NORMAL) return;
        state = StateType.FADE_OUT;
        fade = 0;
        overlay.enabled = true;
    }

    public void setRoom(RoomIdentifier rm) { this.current = rm; }


    #region camShake
    //void ShakeCamera() {
    //    Debug.Log("SHAKING MEISTER");
    //    curAmnt = .1f;//Set default (start) values
    //    curTime = 1f;//Set default (start) values
    //    Debug.Log(curAmnt + ": " + curTime);
    //    //sound.playSound("Shake", true, .1f);
    //    //Only call the coroutine if it isn't currently running. Otherwise, just set the variables.
    //    if (!isShaking) StartCoroutine(Shake());
    //}

    //public void ShakeCamera(float amount, float duration) {
    //    Debug.Log("SHAKING");
    //    shakeAmount = amount;//Add to the current amount.
    //    curAmnt += shakeAmount;//Reset the start amount, to determine percentage.
    //    shakeDuration = duration;//Add to the current time.
    //    curTime += shakeDuration;//Reset the start time.

    //    if (!isShaking) StartCoroutine(Shake());//Only call the coroutine if it isn't currently running. Otherwise, just set the variables.
    //}

    //IEnumerator Shake() {
    //    isShaking = true;

    //    while (curTime > 0.01f) {
    //        Vector3 rotationAmount = Random.insideUnitSphere * curAmnt;//A Vector3 to add to the Local Rotation
    //        rotationAmount.z = 0;//Don't change the Z; it looks funny.

    //        shakePercentage = curTime / shakeDuration;//Used to set the amount of shake (% * startAmount).

    //        curAmnt = shakeAmount * shakePercentage;//Set the amount of shake (% * startAmount).
    //        curTime = Mathf.Lerp(curTime, 0, Time.deltaTime);//Lerp the time, so it is less and tapers off towards the end.

    //        if (smooth)
    //            cam.transform.localRotation = Quaternion.Lerp(cam.transform.localRotation,
    //                Quaternion.Euler(rotationAmount), Time.deltaTime * smoothAmount);
    //        else
    //            cam.transform.localRotation = Quaternion.Euler(rotationAmount);//Set the local rotation the be the rotation amount.
    //        yield return null;
    //    }

    //    transform.localRotation = Quaternion.identity;//Set the local rotation to 0 when done, just to get rid of any fudging stuff.
    //    isShaking = false;
    //}
    #endregion
}
