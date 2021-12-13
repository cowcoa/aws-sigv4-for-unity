using UnityEngine;
using UnityEngine.Networking;

using System.IO;
using System.Collections;
using System.Collections.Generic;

using AWSUtilities;

public class CredentialInfo
{
    public static CredentialInfo CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<CredentialInfo>(jsonString);
    }

    public string accessKeyId;
    public string secretAccessKey;
    public string sessionToken;
    // Temporary credential validity duration.
    // The client should track this time and re-apply when the temporary credential expires.
    public int durationSeconds;
}

public class AWSDataTransferTest : MonoBehaviour
{
    // Image for uploading test.
    string m_UploadImage = "Assets/AWSDataTransfer/Examples/用 鱼测试.jpg";

    // API Gateway credential.
    public string m_UserName = "PXiD_kBa7bqalm7B";
    public string m_Password = "b9G4";
    string m_ApiKey = "nPzTZDYOHt6wRKBIyfpPf31DL2JLNVvx49dIhuKU";
    // API Gateway API URL
    string m_AwsGetUploadCredentialUrl = "https://api.cotecercle.com/v2-dev/credentials/upload";

    // Just for testing.
    public string m_LevelId = "sid_rHmSTB";

    // Use this for initialization
    void Start()
    {
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 300, 100), "Upload Image"))
        {
            if (!File.Exists(m_UploadImage))
            {
                Debug.LogError("Upload image does not exist");
                return;
            }

            Debug.Log("Request: GET " + m_AwsGetUploadCredentialUrl);

            StartCoroutine(GetSTSCredential(m_AwsGetUploadCredentialUrl));
        }
    }

    IEnumerator GetSTSCredential(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            string auth_basic = string.Concat("Basic ", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(string.Format("{0}:{1}", m_UserName, m_Password))));
            webRequest.SetRequestHeader("Authorization", auth_basic);
            webRequest.SetRequestHeader("x-api-key", m_ApiKey);

            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);

                    // Splicing upload URI
                    var endpointUri = AWSDataUpload.SpliceUploadUri(m_UserName, EnvType.Dev, "用 鱼测试.jpg");
                    Debug.Log("Request: PUT " + endpointUri);

                    // Load upload content
                    byte[] uploadContent = File.ReadAllBytes(m_UploadImage);

                    // Parse temporary credentials
                    CredentialInfo credentialInfo = CredentialInfo.CreateFromJSON(webRequest.downloadHandler.text);

                    // Fill in the necessary authentication headers
                    var headers = new Dictionary<string, string>();
                    AWSDataUpload.FillHeaders(ref headers, m_LevelId, endpointUri, uploadContent, credentialInfo.accessKeyId, credentialInfo.secretAccessKey, credentialInfo.sessionToken);

                    StartCoroutine(Upload(endpointUri, uploadContent, headers));
                    break;
            }
        }
    }

    IEnumerator Upload(System.Uri endpointUri, byte[] uploadContent, Dictionary<string, string> headers)
    {
        using (UnityWebRequest www = UnityWebRequest.Put(endpointUri, uploadContent))
        {
            foreach (KeyValuePair<string, string> header in headers)
            {
                www.SetRequestHeader(header.Key, header.Value);
            }

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Upload complete!");
            }
        }
    }
}
