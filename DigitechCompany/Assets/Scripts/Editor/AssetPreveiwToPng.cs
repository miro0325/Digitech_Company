using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class AssetPreviewToPng : EditorWindow
{
    [MenuItem("Tools/Prefab View To Png")]
    public static void Convert()
    {
        var directory = EditorUtility.OpenFolderPanel("Select Save Folder", Application.dataPath, "");

        var size = new Vector2Int(512, 512);
        var camera = Camera.main;
        var rt = new RenderTexture(size.x, size.y, 48);
        camera.targetTexture = rt;

        var selects = Selection.gameObjects;

        foreach(var s in selects) s.SetActive(false);

        GameObject prev = null;
        foreach(var s in selects)
        {
            prev?.SetActive(false);
            s.SetActive(true);

            var screenShot = new Texture2D(size.x, size.y, TextureFormat.ARGB32, false);
            camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
            var pixels = screenShot.GetPixels();
            for(int i = 0; i < pixels.Length; i++)
                if(pixels[i] == camera.backgroundColor)
                    pixels[i] = new Color(1, 1, 1, 0);
            screenShot.SetPixels(pixels);
            screenShot.Apply();
            SaveTextureToPNGFile(screenShot, directory, s.name);
            prev = s;
        }

        camera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);
    }

    public static void SaveTextureToPNGFile(Texture2D texture, string directory, string file)
    {
        var bytes = texture.EncodeToPNG();
        File.WriteAllBytes(directory + '/' + file + ".png", bytes);
        Debug.Log("File was succefully save at: " + directory + '/' + file + ".png");
    }
}