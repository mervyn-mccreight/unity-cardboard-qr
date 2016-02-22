using System.Collections;
using System.Collections.Generic;
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
        public const float ToastLengthLong = 5.0f;

        private WebCamTexture _webcamTexture;
        private WebCamDevice _backFacing;
        private Thread _qrCodeThread;
        private bool _runThread = true;
        private QrCodeCollection _qrCodeCollection;

        private Color32[] _pixels;

        public static CameraScript CameraScriptInstance;

        private AudioSource _coin;

        private string _toastMessage;
        private float _toastDuration;

        // Use this for initialization
        protected virtual void Start()
        {
            GlobalState.Instance.SceneToSwitchTo = Config.Scenes.None;

            CameraScriptInstance = this;

            _coin = GetComponent<AudioSource>();

            _qrCodeCollection = new QrCodeCollection();
            GlobalState.Instance.Reset();

            Application.RequestUserAuthorization(UserAuthorization.WebCam);
            if (Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                Debug.Log("access to webcam granted!");
                Debug.Log("#WebCamDevices: " + WebCamTexture.devices.GetLength(0));

                if (GlobalState.Instance.WebCamTexture == null)
                {
                    foreach (WebCamDevice device in WebCamTexture.devices)
                    {
                        Debug.Log("WebCamDevice: " + device.name);
                        Debug.Log("FrontFacing? " + device.isFrontFacing);
                        if (!device.isFrontFacing)
                        {
                            _backFacing = device;
                        }
                    }

                    // TODO: make prettier. this is dirty
                    _webcamTexture = new WebCamTexture(_backFacing.name, 1024, 768);
                    _webcamTexture.Play();
                    if (_webcamTexture.width != 1024 || _webcamTexture.height != 768)
                    {
                        _webcamTexture.Stop();
                        _webcamTexture = new WebCamTexture(_backFacing.name, 640, 480);
                        _webcamTexture.Play();
                    }

                    GlobalState.Instance.WebCamTexture = _webcamTexture;
                }
                else
                {
                    _webcamTexture = GlobalState.Instance.WebCamTexture;
                    _webcamTexture.Play();
                }

                GetComponent<Renderer>().material.SetTexture("_MainTex", _webcamTexture);
                Debug.Log(string.Format("Actual camera dimens: {0}x{1}", _webcamTexture.width, _webcamTexture.height));

                GlobalState.Instance.CamWidth = _webcamTexture.width;
                GlobalState.Instance.CamHeight = _webcamTexture.height;
                float camRatio = (float) _webcamTexture.width/_webcamTexture.height;
                float screenRatio = (float) Screen.width/Screen.height;

                // Scale plane so it fills the screen while keeping the camera's aspect ratio.
                // If the camera's aspect ratio differs from the screen's,
                // one side will match exactly and the other side will be larger than the screen's dimension.
                var idealHeight = 0.7f;
                if (screenRatio > camRatio)
                {
                    gameObject.transform.localScale = new Vector3(screenRatio * idealHeight, 1, screenRatio * idealHeight / camRatio);
                }
                else
                {
                    gameObject.transform.localScale = new Vector3(camRatio * idealHeight, 1, idealHeight);
                }

                GlobalState.Instance.PlaneWidth = gameObject.transform.localScale.x*10;
                GlobalState.Instance.PlaneHeight = gameObject.transform.localScale.z*10;

                _qrCodeThread = new Thread(DecodeQr);
                _qrCodeThread.Start();
            }
            else
            {
                Debug.LogError("No User Authorization for Camera Device.");
            }

            if (GlobalState.Instance.AllQuestions.questions.Length == GlobalState.Instance.CollectedCoinCount())
            {
                SetToastToShow(StringResources.WinToastMessage, ToastLengthLong);
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
                    LuminanceSource lum = new Color32LuminanceSource(_pixels, GlobalState.Instance.CamWidth, GlobalState.Instance.CamHeight);
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

                        if (GlobalState.Instance.AllQuestions.questions.Length == GlobalState.Instance.CollectedCoinCount())
                        {
                            SetToastToShow(StringResources.WinToastMessage, ToastLengthLong);
                        }
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene(Config.SceneName(Config.Scenes.MainMenu));
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
            if (_webcamTexture.isPlaying)
            {
                _pixels = _webcamTexture.GetPixels32();
            }

            var sceneToSwitchTo = GlobalState.Instance.SceneToSwitchTo;
            if (sceneToSwitchTo != Config.Scenes.None)
            {
                GlobalState.Instance.SceneToSwitchTo = Config.Scenes.None;
                SceneManager.LoadScene(Config.SceneName(sceneToSwitchTo));
            }

            ShowToast();
        }

        // Sent to all game objects before the application is quit.
        protected virtual void OnApplicationQuit()
        {
            _runThread = false;
        }

        // This function is called when the MonoBehaviour will be destroyed.
        protected virtual void OnDestroy()
        {
			_runThread = false;
            _qrCodeThread.Abort();
//            _webcamTexture.Stop();    <-- stopping the WebCamTexture is a major performance issue (>500ms lag)
        }

        public void SetToastToShow(string message, float duration)
        {
            _toastMessage = message;
            _toastDuration = duration;
        }

        private void ResetToast()
        {
            _toastMessage = null;
            _toastDuration = 0f;
        }

        private void ShowToast()
        {
            if (_toastMessage != null && _toastDuration > 0f && !Toast.activeInHierarchy)
            {
                Toast.GetComponentInChildren<Text>().text = _toastMessage;
                Toast.SetActive(true);
                StartCoroutine(HideToast(_toastDuration));
                ResetToast();
            }
        }

        private IEnumerator HideToast(float delay)
        {
            yield return new WaitForSeconds(delay);
            Toast.SetActive(false);
        }
    }
}