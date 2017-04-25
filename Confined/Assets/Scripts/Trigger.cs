using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour {

    public bool retriggerable;
    public string condition;
    [HideInInspector] public string scriptFile;

    bool triggered;
    SceneController sc;
    Evaluator evaluator;
    
    void Start() {
        evaluator = FindObjectOfType<Evaluator>();
        sc = FindObjectOfType<SceneController>();
    }

    void OnTriggerEnter(Collider col) {
        GameObject go = col.gameObject;
        if (!go.name.ToLower().Equals("interactspace")) return;
        if (!retriggerable && triggered) return;
        if (evaluator.evaluate(condition)) {
            sc.loadScript(scriptFile);
            triggered = true;
        }
    }
}
