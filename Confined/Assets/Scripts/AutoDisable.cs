using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AutoDisable : MonoBehaviour {

    public static Dictionary<string, GameObject> autoDisabledObjs;
    public static GameObject Find(string name) {
        return autoDisabledObjs.ContainsKey(name) ? autoDisabledObjs[name]:null;
    }

    static AutoDisable() {
        autoDisabledObjs = new Dictionary<string, GameObject>();
    }

    void Awake() {
        gameObject.SetActive(false);
        autoDisabledObjs.Add(gameObject.name, gameObject);
    }
}