using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZXing;
using ZXing.Common;
using ZXing.QrCode.Internal;

namespace Assets.Scripts
{
    public class CameraScript : MonoBehaviour
    {
        public Text UiText;
        public GameObject Toast;
        public const float ToastLength = 2.0f;

        private WebCamTexture _webcamTexture;
        private WebCamDevice _backFacing;
        private Thread _qrCodeThread;
        private bool _runThread = true;
        private QrCodeCollection _qrCodeCollection;

        private Color32[] _pixels;

        public static CameraScript CameraScriptInstance;

        private AudioSource _coin;
        private int _realCamWidth;
        private int _realCamHeight;

        // Use this for initialization
        protected virtual void Start()
        {
            CameraScriptInstance = this;

            _coin = GetComponent<AudioSource>();

            _qrCodeCollection = new QrCodeCollection();
            GlobalState.Instance.Reset();

            Application.RequestUserAuthorization(UserAuthorization.WebCam);
            if (Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                Debug.Log("access to webcam granted!");
                Debug.Log("#WebCamDevices: " + WebCamTexture.devices.GetLength(0));

                foreach (WebCamDevice device in WebCamTexture.devices)
                {
                    Debug.Log("WebCamDevice: " + device.name);
                    Debug.Log("FrontFacing? " + device.isFrontFacing);
                    if (!device.isFrontFacing)
                    {
                        _backFacing = device;
                    }
                }

                _webcamTexture = new WebCamTexture(_backFacing.name, Config.CamWidth, Config.CamHeight);

                GetComponent<Renderer>().material.SetTexture("_MainTex", _webcamTexture);
                _webcamTexture.Play();
                _realCamWidth = _webcamTexture.width;
                _realCamHeight = _webcamTexture.height;

                _qrCodeThread = new Thread(DecodeQr);
                _qrCodeThread.Start();
            }
            else
            {
                Debug.LogError("No User Authorization for Camera Device.");
            }
        }

        // Function for just fetching the QR-Codes in the WebCamTexture-Image
        // It stores the Data in a QrCodeData-Object.
        // It uses the "pixels" field as the image-source, since we can not 
        // access the Unity-API from another thread than the main-thread.
        private void DecodeQr()
        {
            while (_runThread)
            {
                // waiting.
                if (_pixels != null)
                {
                    LuminanceSource lum = new Color32LuminanceSource(_pixels, _realCamWidth, _realCamHeight);
                    HybridBinarizer bin = new HybridBinarizer(lum);
                    BinaryBitmap binBip = new BinaryBitmap(bin);
                    BitMatrix matrix = binBip.BlackMatrix;
                    Detector detector = new Detector(matrix);

                    DetectorResult result = detector.detect();
                    _qrCodeCollection.UpdateData(result);
                }
            }
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            if (Input.touchCount > 0)
            {
                if (GlobalState.Instance.CurrentCoin >= 0)
                {
                    if (GlobalState.Instance.CollectCoin())
                    {
                        _coin.Play();
                        _qrCodeCollection.DestroyDataObject(GlobalState.Instance.CurrentCoin);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene(Config.MainMenuScene);
            }


            // destroy data which is marked as to be destroyed from the thread.
            _qrCodeCollection.DestroyMarkedOnUpdate();

            // first draw upon the old data.
            UiText.text = _qrCodeCollection.ToString();

            // update the qr code object drawings.
            _qrCodeCollection.Update(transform);

            //TODO:
            // pixels hat ein boolean, dass sagt ob der aktuelle stand schon bearbeitet wurde.
            // pixels wird nur neu beschrieben, wenn der qrcode thread gerade NICHT in der analyse ist.
            // nach der analyse setzt der qrcode thread das boolean auf true.
            // der qr code thread bearbeitet pixels nur, wenn es bisher nicht analysiert wurde (bool false).
            // idee: synchronisation über das wrapper objekt mit bool und pixels.

            // then fetch new data for new calculations.
            _pixels = _webcamTexture.GetPixels32();
        }

        // Sent to all game objects before the application is quit.
        protected virtual void OnApplicationQuit()
        {
            _runThread = false;
        }

        // This function is called when the MonoBehaviour will be destroyed.
        protected virtual void OnDestroy()
        {
            _qrCodeThread.Abort();
            _webcamTexture.Stop();
        }

        public void ShowToast(string message, float delay)
        {
            if (!Toast.activeInHierarchy)
            {
                Toast.GetComponentInChildren<Text>().text = message;
                Toast.SetActive(true);
                StartCoroutine(HideToast(delay));
            }
        }

        private IEnumerator HideToast(float delay)
        {
            yield return new WaitForSeconds(delay);
            Toast.SetActive(false);
        }
    }
}