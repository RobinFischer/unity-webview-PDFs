using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class PDFViewer : MonoBehaviour
{
    private WebViewObject webViewObject;
    public string fileName = "sample.pdf"; // PDF file in StreamingAssets

    void Start()
    {
        StartCoroutine(LoadPDFWithPDFJS());
    }

    IEnumerator LoadPDFWithPDFJS()
    {
        // Debug paths
        string pdfSourcePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        string localPDFPath = System.IO.Path.Combine(Application.persistentDataPath, fileName);
        Debug.Log($"PDF Source Path: {pdfSourcePath}");
        Debug.Log($"Local PDF Path: {localPDFPath}");

        // Copy PDF to persistentDataPath if necessary
        if (!System.IO.File.Exists(localPDFPath))
        {
            Debug.Log("PDF not found in persistent path, copying...");
            if (Application.platform == RuntimePlatform.Android)
            {
                using (UnityWebRequest www = UnityWebRequest.Get(pdfSourcePath))
                {
                    yield return www.SendWebRequest();
                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        System.IO.File.WriteAllBytes(localPDFPath, www.downloadHandler.data);
                        Debug.Log("PDF copied successfully to persistent path");
                    }
                    else
                    {
                        Debug.LogError($"Failed to load PDF from StreamingAssets: {www.error}");
                        yield break;
                    }
                }
            }
            else
            {
                System.IO.File.Copy(pdfSourcePath, localPDFPath);
                Debug.Log("PDF copied successfully to persistent path");
            }
        }

        // Initialize WebView
        webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
        webViewObject.Init(
            cb: (msg) => {
                Debug.Log($"WebView Message: {msg}");
            },
            err: (msg) => {
                Debug.LogError($"WebView Error: {msg}");
            }
        );

        webViewObject.SetMargins(0, 0, 0, 0);
        webViewObject.SetVisibility(true);

        // Load the PDF.js viewer HTML
        string pdfViewerURL = System.IO.Path.Combine(Application.streamingAssetsPath, "pdfjs/web/viewer.html");
        if (Application.platform == RuntimePlatform.Android)
        {
            pdfViewerURL = "file:///android_asset/pdfjs/web/viewer.html";
        }
        
        // Construct the URL with the PDF file parameter
        string localPDFURL = "file://" + localPDFPath;
        string finalURL = $"{pdfViewerURL}?file={UnityWebRequest.EscapeURL(localPDFURL)}";
        Debug.Log($"Loading URL: {finalURL}");

        webViewObject.LoadURL(finalURL);
    }
}
