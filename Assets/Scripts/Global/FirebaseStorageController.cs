using Assets.Scripts.Models;
using Firebase.Extensions;
using Firebase.Storage;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class FirebaseStorageController : MonoBehaviour
{
    public static FirebaseStorageController Instance { get; set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Firebase storage service.
    /// </summary>
    private FirebaseStorage Storage;

    /// <summary>
    /// Bucket in storage service.
    /// </summary>
    private StorageReference StorageRef;

    [Header("Bucket main url.")]
    public string BucketUrl = $"gs://test-1576579639708.appspot.com/";

    [Header("Where users upload will be saved.")]
    public string SubFolderOfUsers = $"UserPlays";

    private void Start()
    {
        // Get a reference to the storage service, using the default Firebase App
        Storage = FirebaseStorage.DefaultInstance;

        // Create a storage reference from our storage service
        StorageRef = Storage.GetReferenceFromUrl(BucketUrl);
    }

    #region Download For Config
    public void DownloadUrlForConfig(string fileName, Action<LevelEditorModel> onCallBack)
    {
        // Fetch the download URL
        Storage.GetReferenceFromUrl($"{BucketUrl}{SubFolderOfUsers}/{fileName}").GetDownloadUrlAsync().ContinueWithOnMainThread(configUrl =>
        {
            if (!configUrl.IsFaulted && !configUrl.IsCanceled)
                StartCoroutine(DownloadConfigFile(configUrl.Result.ToString(), onCallBack));
            else // if error exists then return null.
                onCallBack.Invoke(null);
        });
    }

    public IEnumerator DownloadConfigFile(string downloadUrl, Action<LevelEditorModel> onCallBack)
    {
        // We download the texture file.
        UnityWebRequest request = UnityWebRequest.Get(downloadUrl);

        // We wait until download is completed.
        yield return request.SendWebRequest();

        // Check the state if error exists do somthing if not use texture.
        if (request.isNetworkError || request.isHttpError)
            onCallBack.Invoke(null);
        else
        {
            // We return to the callback.
            onCallBack.Invoke(JsonUtility.FromJson<LevelEditorModel>(request.downloadHandler.text));
        }
    }

    #endregion

    #region Download For Texture.

    public void DownloadUrlForTexture(string fileName, Action<Texture2D> onCallBack)
    {
        // Fetch the download URL
        Storage.GetReferenceFromUrl($"{BucketUrl}/{SubFolderOfUsers}/{fileName}").GetDownloadUrlAsync().ContinueWithOnMainThread(configUrl =>
        {
            if (!configUrl.IsFaulted && !configUrl.IsCanceled)
                StartCoroutine(DownloadTextureFile(configUrl.Result.ToString(), onCallBack));
            else // if error exists then return null.
                onCallBack.Invoke(null);
        });
    }

    public IEnumerator DownloadTextureFile(string downloadUrl, Action<Texture2D> onCallBack)
    {
        // We download the texture file.
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(downloadUrl);

        // We wait until download is completed.
        yield return request.SendWebRequest();

        // Check the state if error exists do somthing if not use texture.
        if (request.isNetworkError || request.isHttpError)
            onCallBack.Invoke(null);
        else
        {
            // We get the texture.
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;

            // We return to the callback.
            onCallBack.Invoke(texture);
        }
    }

    #endregion

    #region Upload For Texture

    public void UploadImage(Texture2D texture, Action<Task<StorageMetadata>> onCallBack)
    {
        // We get the bytes of texture.
        byte[] textureInformations = texture.EncodeToJPG();

        // Create a reference to the file you want to upload
        StorageReference riversRef = StorageRef.Child($"{SubFolderOfUsers}/{FileBrowserController.Instance.GetRandomFilename()}.data");

        // Upload the file to the path "images/rivers.jpg"
        riversRef.PutBytesAsync(textureInformations).ContinueWithOnMainThread((Task<StorageMetadata> task) => onCallBack.Invoke(task));
    }

    #endregion

    #region Upload For Config

    public void UploadConfig(LevelEditorModel levelData, Action<Task<StorageMetadata>> onCallBack)
    {
        // Create a reference to the file you want to upload
        StorageReference riversRef = StorageRef.Child($"{SubFolderOfUsers}/{levelData.ConfigFileName}.config");

        // As json.
        string jsonData = JsonUtility.ToJson(levelData);

        // Bytes of data.
        byte[] jsonDataBytes = Encoding.UTF8.GetBytes(jsonData);

        // We upload the data.
        riversRef.PutBytesAsync(jsonDataBytes).ContinueWithOnMainThread((Task<StorageMetadata> task) => onCallBack.Invoke(task));
    }

    #endregion
    public void DownloadSize(string key, Action<long> onCallBack)
    {
        // Fetch the download URL
        Storage.GetReferenceFromUrl($"{StorageRef}/{key}").GetMetadataAsync().ContinueWith(metaData =>
        {
            if (!metaData.IsFaulted && !metaData.IsCanceled)
                onCallBack.Invoke(metaData.Result.SizeBytes);
        });
    }

}
