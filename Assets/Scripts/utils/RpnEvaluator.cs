using UnityEngine;
using UnityEditor;
using System.IO;

namespace Rpn
// to start, use for HP and counts from enemies.json
{
    public class RpnEvaluator {
        public static string JSONloader(string filename) {
            // hard-code file location for now 
            string path = "Assets/Resources/enemies.json";
            TextAsset json = Resources.Load<TextAsset>(path);
            StreamReader reader = new StreamReader(path);
            Debug.Log(reader.ReadToEnd());
            reader.Close();
            // return test string for now
            return "Test";

        }

    }
}

