using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Trigger))]
public class TriggerEdit : Editor {

    private int index = -1;
    GameObject go;
    List<string> scrOpt;

    void OnEnable() {
        scrOpt = new List<string>(Vars.SCRIPT_LIST.Keys);
        Trigger rI = ((Trigger)target);
        index = scrOpt.IndexOf(rI.scriptFile);
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        Trigger iB = (Trigger)target;

        int i = EditorGUILayout.Popup("Script Source", index < 0 ? 0 : index, scrOpt.ToArray());
        if (i != index) {
            iB.scriptFile = scrOpt[i];
            index = i;
        }
    }
}
