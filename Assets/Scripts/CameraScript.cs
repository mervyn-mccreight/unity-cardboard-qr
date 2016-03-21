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
    /// <summary>
    /// Handles camera scene UI. Is attached to Unity CameraScene.CardboardMain.Head.Main_Camera.Plane.
    /// </summary>
    public class CameraScript : MonoBehaviour
    {
        /// <summary>
        /// Toast game object to display short messages to the user. Referenced in CameraScene.CardboardMain.Head.Main_Camera.Plane.
        /// </summary>
        public GameObject Toast;
        public const float ToastLength = 2.0f;
        public const float ToastLengthLong = 5.0f;
        private string _toastMessage;
        private float _toastDuration;

        private WebCamTexture _webcamTexture;
        private WebCamDevice _backFacing;
        private Thread _qrCodeThread;
        private bool _runThread = true;
        private QrCodeCollection _qrCodeCollection;

        private Color32[] _pixels;

        public static CameraScript CameraScriptInstance;

        private AudioSource _coin;

        protected virtual void Start()
        {
            GlobalState.Instance.SceneToSwitchTo = Config.Scenes.None;

            // Keep a static reference to this class to be able to display toast messages from other components (namely QrCodeCollection.cs).
            CameraScriptInstance = this;

            _coin = GetComponent<AudioSource>();

            _qrCodeCollection = new QrCodeCollection();

            // Reset the global state current question and coin every time we restart the camera scene.
            GlobalState.Instance.Reset();

            Application.RequestUserAuthorization(UserAuthorization.WebCam);
            if (Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                Debug.Log("access to webcam granted!");
                Debug.Log("#WebCamDevices: " + WebCamTexture.devices.GetLength(0));

                if (GlobalState.Instance.WebCamTexture == null)
                {
                    // Find a backfacing camera device.
                    foreach (WebCamDevice device in WebCamTexture.devices)
                    {
                        Debug.Log("WebCamDevice: " + device.name);
                        Debug.Log("FrontFacing? " + device.isFrontFacing);
                        if (!device.isFrontFacing)
                        {
                            _backFacing = device;
                        }
                    }

                    // Try to obtain a 1024x768 texture from the webcam.
                    _webcamTexture = new WebCamTexture(_backFacing.name, 1024, 768);
                    _webcamTexture.Play();

                    // The device might not support the requested resolution, so we try again with a lower one.
                    if (_webcamTexture.width != 1024 || _webcamTexture.height != 768)
                    {
                        _webcamTexture.Stop();
                        _webcamTexture = new WebCamTexture(_backFacing.name, 640, 480);
                        _webcamTexture.Play();
                    }

                    // Keep a global reference to the WebCamTexture to speed up scene initialization next time.
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
                    gameObject.transform.localScale = new Vector3(screenRatio*idealHeight, 1,
                        screenRatio*idealHeight/camRatio);
                }
                else
                {
                    gameObject.transform.localScale = new Vector3(camRatio*idealHeight, 1, idealHeight);
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

            // Check win condition: if the user has already collected all the coins, show a toast.
            if (GlobalState.Instance.AllQuestions.questions.Length == GlobalState.Instance.CollectedCoinCount())
            {
                SetToastToShow(StringResources.WinToastMessage, ToastLengthLong);
            }
        }

        /// <summary>
        /// Function for just fetching the QR-Codes in the WebCamTexture-Image.
        /// It stores the Data in a QrCodeData-Object.
        /// It uses the "pixels" field as the image-source, since we can not
        /// access the Unity-API from another thread than the main-thread.
        /// </summary>
        private void DecodeQr()
        {
            while (_runThread)
            {
                // waiting.
                if (_pixels != null)
                {
                    // Create a BitMatrix from the Color32[] of the camear image, so that XZing can detect QR-codes.
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

        protected virtual void Update()
        {
            // Handle user touching the screen while the coin is visible (collecting the coin).
            if (Input.touchCount > 0)
            {
                if (GlobalState.Instance.CurrentCoin >= 0)
                {
                    if (GlobalState.Instance.CollectCoin())
                    {
                        _coin.Play();
                        _qrCodeCollection.DestroyDataObject(GlobalState.Instance.CurrentCoin);

                        // Win condition.
                        if (GlobalState.Instance.AllQuestions.questions.Length ==
                            GlobalState.Instance.CollectedCoinCount())
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

            // Destroy data which is marked as to be destroyed from the thread.
            _qrCodeCollection.DestroyMarkedOnUpdate();

            // Update the qr code object drawings.
            _qrCodeCollection.Update(transform);

            //TODO:
            // pixels hat ein boolean, dass sagt ob der aktuelle stand schon bearbeitet wurde.
            // pixels wird nur neu beschrieben, wenn der qrcode thread gerade NICHT in der analyse ist.
            // nach der analyse setzt der qrcode thread das boolean auf true.
            // der qr code thread bearbeitet pixels nur, wenn es bisher nicht analysiert wurde (bool false).
            // idee: synchronisation über das wrapper objekt mit bool und pixels.

            // Then fetch new data for new calculations.
            if (_webcamTexture.isPlaying)
            {
                _pixels = _webcamTexture.GetPixels32();
            }

            // If the QR code detection has discovered a question, it sets the scene to switch to, which is handled in the update loop.
            // This is needed, because switching scenes can only be done from Unity main thread.
            var sceneToSwitchTo = GlobalState.Instance.SceneToSwitchTo;
            if (sceneToSwitchTo != Config.Scenes.None)
            {
                GlobalState.Instance.SceneToSwitchTo = Config.Scenes.None;
                SceneManager.LoadScene(Config.SceneName(sceneToSwitchTo));
            }

            // Check if a toast has been requested and show it (equivalent to scene switching, for thread-safety reasons).
            ShowToast();
        }

        protected virtual void OnApplicationQuit()
        {
            _runThread = false;
        }

        protected virtual void OnDestroy()
        {
            _runThread = false;
            _qrCodeThread.Abort();
//            _webcamTexture.Stop();    <-- stopping the WebCamTexture when switching scenes is a major performance issue (>500ms lag)
        }

        /// <summary>
        /// Request a toast message be shown on next frame update.
        /// </summary>
        /// <param name="message">Message to be shown</param>
        /// <param name="duration">Duration the toast should be visible in seconds</param>
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