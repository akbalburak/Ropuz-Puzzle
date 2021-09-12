using Assets.Scripts.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class FileBrowserController : MonoBehaviour
{
    public static FileBrowserController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Custom application data folder.
    /// </summary>
    public string CustomLevelFolder => $"{Application.persistentDataPath}/CustomLevels/";

    public void PickAnImage(Action<Texture2D> onSelectedTexture)
    {
        // Get image returns the permissiono also.
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
            // We make sure path is exists.
            if (path != null)
            {
                // Create Texture from selected image
                Texture2D texture = NativeGallery.LoadImageAtPath(path, markTextureNonReadable: false);

                // if texture not exists then print error.
                if (texture == null)
                {
                    Debug.Log("Couldn't load texture from " + path);

                    // Then return.
                    return;
                }

                // We are returning the selected texture.
                onSelectedTexture.Invoke(texture);
            }
        });
    }

    public void SaveTextureFile(string filename, Texture2D texture)
    {
        // if the directory not exists in the destination we will crate.
        if (!Directory.Exists(CustomLevelFolder))
            Directory.CreateDirectory(CustomLevelFolder);

        // We get texture bytes.
        byte[] textureAsBytes = NativeGallery.GetTextureBytes(texture, true);

        // We are writing to disk.
        using (FileStream fileStream = File.OpenWrite($"{CustomLevelFolder}{filename}"))
            fileStream.Write(textureAsBytes, 0, textureAsBytes.Length);
    }

    public byte[] LoadFile(string fileName) => File.ReadAllBytes($"{CustomLevelFolder}{fileName}");

    public void SaveConfigFile(string filename, string data)
    {
        // if the directory not exists in the destination we will crate.
        if (!Directory.Exists(CustomLevelFolder))
            Directory.CreateDirectory(CustomLevelFolder);

        // We convert to bytes.
        byte[] bytes = Encoding.UTF8.GetBytes(data);

        // We are writing to disk.
        using (FileStream fileStream = File.OpenWrite($"{CustomLevelFolder}{filename}"))
            fileStream.Write(bytes, 0, bytes.Length);
    }

    public List<LevelEditorModel> LoadConfigFiles(string filter)
    {
        // if directory not exists return empty list.
        if (!Directory.Exists(CustomLevelFolder))
            return new List<LevelEditorModel>();

        // File list.
        string[] fileList = Directory.GetFiles($"{CustomLevelFolder}", filter);

        // Editor levels.
        List<LevelEditorModel> levelEditorModels = new List<LevelEditorModel>();

        // We loop in the directory files.
        foreach (string file in fileList)
        {
            // We read the files.
            byte[] fileBytes = File.ReadAllBytes(file);

            // Level information.
            LevelEditorModel levelData = JsonUtility.FromJson<LevelEditorModel>(Encoding.UTF8.GetString(fileBytes));

            // We set the file path to delete or update file.
            levelData.ConfigFileName = Path.GetFileName(file);

            // We add to the list.
            levelEditorModels.Add(levelData);
        }

        // Levels.
        return levelEditorModels;
    }

    public void RemoveFile(string fileName)
    {
        // We delete the given file.
        if (File.Exists($"{CustomLevelFolder}/{fileName}"))
            File.Delete($"{CustomLevelFolder}/{fileName}");
    }
}
