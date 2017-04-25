using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Vars {

    public static Dictionary<string, string> SCRIPT_LIST;
    public static bool debug = true;

    static Vars() {
        SCRIPT_LIST = new Dictionary<string, string>();
        loadList(Application.streamingAssetsPath +"/Dialogue", ".cfsc", SCRIPT_LIST);

    }

    public static float clamp(float f, float min = 0, float max = 1) {
        return f > max ? max : f < min ? min : f;
    }

    private static void loadList(string directory, string ext, object list) {
        DirectoryInfo baseFolder = new DirectoryInfo(directory);
        DirectoryInfo[] dirs = baseFolder.GetDirectories();
        foreach (DirectoryInfo dir in dirs)
            loadList(dir.FullName, ext, list);
        FileInfo[] files = baseFolder.GetFiles();

        foreach (FileInfo file in files) {
            if (file.Extension.Contains(ext)) {
                if (list.GetType() == typeof(Dictionary<string, string>)) {
                    ((Dictionary<string, string>)list).Add(file.Name.Replace(ext, ""), file.FullName);
                }else if (list.GetType() == typeof(List<string>))
                ((List<string>)list).Add(file.Name);
            } 
        }
    }

    public static bool isNumeric(string s) {
        double retNum;

        bool isNum = double.TryParse(s,
            System.Globalization.NumberStyles.Any,
            System.Globalization.NumberFormatInfo.InvariantInfo,
            out retNum);
        return isNum;
    }

    public static bool isBool(string s) {
        if (s == null) return false;
        return "true".Equals(s.ToLower().Trim()) || "false".Equals(s.ToLower().Trim());
    }
}
