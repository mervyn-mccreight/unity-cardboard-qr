using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZXing;
using ZXing.Common;
using ZXing.QrCode.Internal;

namespace Assets.Scripts
{
    public class TextureScript : MonoBehaviour
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
        private static readonly int CamWidth = 1024;
        private static readonly int CamHeight = 768;

        private static readonly float KeepAliveTime = 0.5f;
        private static readonly float InterpolationSpeed = 2f;

        public static TextureScript TextureScriptInstance;

        private AudioSource _coin;

        // Use this for initialization
        protected virtual void Start()
        {
            TextureScriptInstance = this;

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
                    Debug.Log("WebCamDevice: " + device.name);
                    Debug.Log("FrontFacing? " + device.isFrontFacing);
                    if (!device.isFrontFacing)
                    {
                        _backFacing = device;
                    }
                }

                _webcamTexture = new WebCamTexture(_backFacing.name, CamWidth, CamHeight);

                GetComponent<Renderer>().material.SetTexture("_MainTex", _webcamTexture);
                _webcamTexture.Play();

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
                    // running.
                    LuminanceSource lum = new Color32LuminanceSource(_pixels, CamWidth, CamHeight);
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

        // Class for Mapping QR-Code Data to a Unity GameObject.
        private class QrCodeData
        {
            private ResultPoint[] _target;
            private GameObject _model;
            public int Id { get; private set; }
            private DateTime _time;
            private DateTime _destroyTime = DateTime.MinValue;
            private bool _forceDestroy;

            public QrCodeData(ResultPoint[] resultPoints, GameObject model, int id)
            {
                _target = resultPoints;
                _model = model;
                Id = id;
                _time = DateTime.UtcNow;
            }

            public void Update(ResultPoint[] newResultPoints)
            {
                _target = newResultPoints;
                _time = DateTime.UtcNow;
                _destroyTime = DateTime.MinValue;
            }

            public void SetModel(GameObject model)
            {
                _model = model;
            }

            public GameObject GetModel()
            {
                return _model;
            }

            public void ForceMarkAsDestroy()
            {
                _forceDestroy = true;
            }

            public void MarkAsDestroy()
            {
                if (_destroyTime == DateTime.MinValue)
                {
                    _destroyTime = DateTime.UtcNow;
                }
            }

            public bool IsMarkedAsDestroy()
            {
                if (_forceDestroy)
                {
                    return true;
                }

                if (_destroyTime == DateTime.MinValue)
                {
                    return false;
                }

                return DateTime.UtcNow.Subtract(_destroyTime).TotalSeconds > KeepAliveTime;
            }

            public override string ToString()
            {
                var result = _target.Aggregate("",
                    (accumulator, resultPoint) =>
                        accumulator + string.Format("({0}, {1})", resultPoint.X, resultPoint.Y) + Environment.NewLine);

                var modelString = "none";
                if (_model != null)
                {
                    modelString = _model.ToString();
                }

                return result + "Type =" + modelString + Environment.NewLine;
            }

            private Vector3 Diagonal()
            {
                var p1 = new Vector3(_target[0].X, _target[0].Y);
                var p2 = new Vector3(_target[1].X, _target[1].Y);
                var p3 = new Vector3(_target[2].X, _target[2].Y);

                var ab = p2 - p1;
                var ac = p3 - p1;
                var bc = p3 - p2;

                Vector3 max;

                //// find the longest line.
                if (ab.magnitude > ac.magnitude)
                {
                    max = ab;
                }
                else
                {
                    max = ac;
                }

                if (bc.magnitude > max.magnitude)
                {
                    max = bc;
                }

                return max;
            }

            public Vector3 CenterToPlane()
            {
                var p1 = new Vector3(_target[0].X, _target[0].Y);
                var p2 = new Vector3(_target[1].X, _target[1].Y);
                var p3 = new Vector3(_target[2].X, _target[2].Y);

                var ab = p2 - p1;
                var ac = p3 - p1;

                var max = Diagonal();

                Vector3 position;
                if (max == ab || max == ac)
                {
                    position = p1 + (max/2);
                }
                else
                {
                    position = p2 + (max/2);
                }

                return new Vector3((position.x - (float) CamWidth/2)/CamWidth*1.334f*10*-1, 2,
                    (position.y - (float) CamHeight/2)/CamHeight*10);
            }

            private Vector3 ScaleFactor()
            {
                return new Vector3(Diagonal().magnitude/100f, Diagonal().magnitude/100f*0.05f, Diagonal().magnitude/100f);
            }

            public void UpdateModel()
            {
                GetModel().transform.localPosition = Interpolate(GetModel().transform.localPosition, CenterToPlane());
                GetModel().transform.localScale = Interpolate(GetModel().transform.localScale, ScaleFactor());
                GetModel().transform.Rotate(Vector3.forward*Time.smoothDeltaTime*100f);
            }

            private Vector3 Interpolate(Vector3 from, Vector3 to)
            {
                var traveledDistance = ((float) DateTime.UtcNow.Subtract(_time).TotalSeconds)*InterpolationSpeed;
                var totalDistance = Vector3.Distance(from, to);

                return Vector3.Lerp(from, to, traveledDistance/totalDistance);
            }
        }

        private class QrCodeCollection
        {
            private readonly List<QrCodeData> _data = new List<QrCodeData>();
            private readonly Decoder _decoder = new Decoder();
            private string _contentString = "ERROR";

            // call this update in the qr code detection thread.
            public void UpdateData(DetectorResult result)
            {
                if (result == null)
                {
                    // if there is no qr-code in the image, clear the area.
                    _data.ForEach(delegate(QrCodeData obj)
                    {
                        // mark destroy all models attached to qrcodedata before cleaning the list.
                        obj.MarkAsDestroy();
                    });

                    return;
                }

                var points = result.Points;

                if (_data.Count == 0)
                {
                    var decoderResult = _decoder.decode(result.Bits, null);
                    if (decoderResult != null)
                    {
                        _contentString = decoderResult.Text;
                        var dataFromJson = JsonUtility.FromJson<Data>(_contentString);

                        switch (dataFromJson.type)
                        {
                            case DataType.Question:
                                if (!GlobalState.Instance.IsCoinUnlocked(dataFromJson.id))
                                {
                                    GlobalState.Instance.CurrentQuestion = dataFromJson.ToQuestion();
                                    SceneManager.LoadScene(Config.QuestionScene);
                                }
                                else
                                {
                                    TextureScriptInstance.ShowToast("Already unlocked!", ToastLength);
                                }
                                break;
                            case DataType.Coin:
                                if (GlobalState.Instance.IsCoinUnlocked(dataFromJson.id))
                                {
                                    if (!GlobalState.Instance.IsCoinCollected(dataFromJson.id))
                                    {
                                        GlobalState.Instance.CurrentCoin = dataFromJson.id;
                                        // null here, since we can not access unity api to create a game object yet.
                                        _data.Add(new QrCodeData(points, null, dataFromJson.id));
                                    }
                                }
                                else
                                {
                                    TextureScriptInstance.ShowToast("Answer the question first!", ToastLength);
                                }
                                break;
                        }
                    }
                    else
                    {
                        _contentString = "ERROR";
                    }
                }
                else
                {
                    var enumerator = _data.GetEnumerator();
                    enumerator.MoveNext();
                    if (enumerator.Current != null)
                    {
                        enumerator.Current.Update(points);
                    }
                }
            }

            public override string ToString()
            {
                if (_data.Count == 0)
                {
                    return "[]" + "\n" + _contentString;
                }

                var result = "[" + Environment.NewLine;
                result = result + string.Join(",", _data.ConvertAll(x => x.ToString()).ToArray());
                result = result + "]";
                result = result + "\n" + _contentString;
                return result;
            }

            public void DestroyDataObject(int id)
            {
                _data.ForEach(delegate(QrCodeData obj)
                {
                    if (obj.Id == id)
                    {
                        obj.ForceMarkAsDestroy();
                    }
                });
            }

            // Destroy the marked objects.
            // Call this only from the Unity mainthread.
            public void DestroyMarkedOnUpdate()
            {
                _data.ForEach(delegate(QrCodeData obj)
                {
                    if (obj.IsMarkedAsDestroy())
                    {
                        GlobalState.Instance.CurrentCoin = -1;
                        if (obj.GetModel() == null)
                        {
                            return;
                        }
                        Destroy(obj.GetModel());
                    }
                });

                _data.RemoveAll(item => item.IsMarkedAsDestroy());
            }

            // call this method in Update from Unity!!
            public void Update(Transform parent)
            {
                _data.ForEach(delegate(QrCodeData code)
                {
                    // since we can not access Unity-API from non-main threads
                    // we have to create the game object referred by the code later in here.
                    if (code.GetModel() == null)
                    {
                        var coin = Instantiate(Resources.Load("Coin")) as GameObject;
                        if (coin != null)
                        {
                            coin.transform.SetParent(parent);
                            coin.transform.localPosition = code.CenterToPlane();
                            code.SetModel(coin);
                        }
                        return;
                    }

                    code.UpdateModel();
                });
            }
        }
    }
}