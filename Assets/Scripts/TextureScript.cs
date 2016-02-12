using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZXing;
using ZXing.Common;
using ZXing.QrCode.Internal;

namespace Assets.Scripts
{
    public class TextureScript : MonoBehaviour {
        public Text uiText;
        public GameObject Toast;
        public const float ToastLength = 2.0f;

        private WebCamTexture webcamTexture;
        private WebCamDevice backFacing;
        private Thread qrCodeThread;
        private bool runThread = true;
        private QRCodeCollection qrCodeCollection;

        private Color32[] pixels = null;
        private static int CAM_WIDTH = 1024;
        private static int CAM_HEIGHT = 768;

        private static float KEEP_ALIVE_TIME = 0.5f;
        private static float INTERPOLATION_SPEED = 2f;

        public static TextureScript TextureScriptInstance;

        private AudioSource coin;

        // Use this for initialization
        void Start()
        {
            TextureScriptInstance = this;

            coin = GetComponent<AudioSource>();

            qrCodeCollection = new QRCodeCollection();
            GlobalState.Reset();

            Application.RequestUserAuthorization(UserAuthorization.WebCam);
            if (Application.HasUserAuthorization(UserAuthorization.WebCam)) {
                Debug.Log("access to webcam granted!");
                Debug.Log("#WebCamDevices: " + WebCamTexture.devices.GetLength(0).ToString());

                foreach (WebCamDevice device in WebCamTexture.devices) {
                    Debug.Log("WebCamDevice: " + device.name);
                    Debug.Log("WebCamDevice: " + device.name);
                    Debug.Log("FrontFacing? " + device.isFrontFacing);
                    if (!device.isFrontFacing) {
                        backFacing = device;
                    }
                }

                webcamTexture = new WebCamTexture(backFacing.name, CAM_WIDTH, CAM_HEIGHT);
   
                GetComponent<Renderer>().material.SetTexture("_MainTex", webcamTexture);
                webcamTexture.Play();

                qrCodeThread = new Thread(DecodeQR);
                qrCodeThread.Start();

            } else {
                Debug.LogError("No User Authorization for Camera Device.");
            }
        }

        // Function for just fetching the QR-Codes in the WebCamTexture-Image
        // It stores the Data in a QrCodeData-Object.
        // It uses the "pixels" field as the image-source, since we can not 
        // access the Unity-API from another thread than the main-thread.
        void DecodeQR() {
            while (runThread) {
                // waiting.
                if (pixels != null) {
                    // running.
                    LuminanceSource lum = new Color32LuminanceSource(pixels, CAM_WIDTH, CAM_HEIGHT);
                    HybridBinarizer bin = new HybridBinarizer(lum);
                    BinaryBitmap binBip = new BinaryBitmap(bin);
                    BitMatrix matrix = binBip.BlackMatrix;
                    Detector detector = new Detector(matrix);

                    DetectorResult result = detector.detect();
                    qrCodeCollection.UpdateData(result);
                }
            }
        }
	
        // Update is called once per frame
        void Update () {
            if (Input.touchCount > 0)
            {
                if (GlobalState.CurrentCoin >= 0)
                {
                    if (!GlobalState.CollectedCoins.Contains(GlobalState.CurrentCoin))
                    {
                        GlobalState.CollectedCoins.Add(GlobalState.CurrentCoin);
                        coin.Play();
                        qrCodeCollection.DestroyDataObject(GlobalState.CurrentCoin);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene("MainMenuScene");
            }


            // destroy data which is marked as to be destroyed from the thread.
            this.qrCodeCollection.DestroyMarkedOnUpdate ();

            // first draw upon the old data.
            uiText.text = qrCodeCollection.ToString();

            // update the qr code object drawings.
            this.qrCodeCollection.Update (transform);

            //TODO:
            // pixels hat ein boolean, dass sagt ob der aktuelle stand schon bearbeitet wurde.
            // pixels wird nur neu beschrieben, wenn der qrcode thread gerade NICHT in der analyse ist.
            // nach der analyse setzt der qrcode thread das boolean auf true.
            // der qr code thread bearbeitet pixels nur, wenn es bisher nicht analysiert wurde (bool false).
            // idee: synchronisation über das wrapper objekt mit bool und pixels.

            // then fetch new data for new calculations.
            pixels = webcamTexture.GetPixels32 ();
        }

        // Sent to all game objects before the application is quit.
        void OnApplicationQuit() {
            runThread = false;
        }

        // This function is called when the MonoBehaviour will be destroyed.
        void OnDestroy() {
            Debug.Log("CameraScene OnDestroy!");
            qrCodeThread.Abort();
            webcamTexture.Stop();
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
        private class QRCodeData {
            private ResultPoint[] target;
            private GameObject model;
            public int Id { get; private set; }
            private DateTime time;
            private DateTime destroyTime = DateTime.MinValue;
            private bool forceDestroy;

            public QRCodeData(ResultPoint[] resultPoints, GameObject model, int id) {
                this.target = resultPoints;
                this.model = model;
                Id = id;
                this.time = DateTime.UtcNow;
            }

            public void Update(ResultPoint[] newResultPoints) {
                this.target = newResultPoints;
                this.time = DateTime.UtcNow;
                this.destroyTime = DateTime.MinValue;
            }

            public ResultPoint[] GetPoints() {
                return this.target;
            }

            public void SetModel(GameObject model) {
                this.model = model;
            }

            public GameObject GetModel() {
                return this.model;
            }

            public void ForceMarkAsDestroy()
            {
                this.forceDestroy = true;
            }

            public void MarkAsDestroy() {
                if (this.destroyTime == DateTime.MinValue) {
                    this.destroyTime = DateTime.UtcNow;
                }
            }

            public bool IsMarkedAsDestroy() {
                if (forceDestroy)
                {
                    return true;
                }

                if (this.destroyTime == DateTime.MinValue)
                {
                    return false;
                }

                return DateTime.UtcNow.Subtract(this.destroyTime).TotalSeconds > KEEP_ALIVE_TIME;
            }

            public override string ToString () {
                string result = "";
                foreach (ResultPoint resultPoint in this.target) {
                    result = result + string.Format("({0}, {1})", resultPoint.X, resultPoint.Y) + System.Environment.NewLine;
                }

			
                string modelString = "none";
                if (this.model != null) {
                    modelString = this.model.ToString();
                }

                result = result + "Type =" + modelString + System.Environment.NewLine;
                return result;
            }

            private Vector3 diagonal()
            {
                var p1 = new Vector3(this.target[0].X, this.target[0].Y);
                var p2 = new Vector3(this.target[1].X, this.target[1].Y);
                var p3 = new Vector3(this.target[2].X, this.target[2].Y);

                var ab = p2 - p1;
                var ac = p3 - p1;
                var bc = p3 - p2;

                Vector3 max = new Vector3(0, 0);

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
                var p1 = new Vector3(this.target[0].X, this.target[0].Y);
                var p2 = new Vector3(this.target[1].X, this.target[1].Y);
                var p3 = new Vector3(this.target[2].X, this.target[2].Y);

                var ab = p2 - p1;
                var ac = p3 - p1;

                var max = diagonal();

                Vector3 position = new Vector3(0, 0);
                if (max == ab || max == ac)
                {
                    position = p1 + (max / 2);
                }
                else
                {
                    position = p2 + (max / 2);
                }

                return new Vector3((position.x - CAM_WIDTH / 2) / (float)CAM_WIDTH * 1.334f * 10 * -1, 2, (position.y - CAM_HEIGHT / 2) / (float)CAM_HEIGHT * 10);
            }

            private Vector3 scaleFactor()
            {
                return new Vector3(diagonal().magnitude / 100f, diagonal().magnitude / 100f * 0.05f, diagonal().magnitude / 100f);
            }

            public void UpdateModel()
            {
                GetModel ().transform.localPosition = interpolate (GetModel ().transform.localPosition, CenterToPlane (), this.time, INTERPOLATION_SPEED);
                GetModel ().transform.localScale = interpolate (GetModel ().transform.localScale, scaleFactor (), this.time, INTERPOLATION_SPEED);
                GetModel ().transform.Rotate (Vector3.forward * Time.smoothDeltaTime * 100f);
            }

            private Vector3 interpolate(Vector3 from, Vector3 to, DateTime startTime, float speed) {
                var traveledDistance = ((float)DateTime.UtcNow.Subtract(this.time).TotalSeconds) * INTERPOLATION_SPEED;
                var totalDistance = Vector3.Distance(from, to);

                return Vector3.Lerp(from, to, traveledDistance/totalDistance);
            }
        }

        private class QRCodeCollection {
            private List<QRCodeData> data = new List<QRCodeData>();
            private Decoder decoder = new Decoder();
            private string contentString = "ERROR";

            // call this update in the qr code detection thread.
            public void UpdateData(DetectorResult result) {
                if (result == null) {
                    // if there is no qr-code in the image, clear the area.
                    data.ForEach(delegate(QRCodeData obj) {
                        // mark destroy all models attached to qrcodedata before cleaning the list.
                        obj.MarkAsDestroy();
                    });

                    return;
                }

                ResultPoint[] points = result.Points;

                if (data.Count == 0) {
                    // TODO: determine QRCode content.
                    // If question -> load question UI scene
                    // If coin -> check if coin was unlocked, then display coin model (this part is already implemented here)

                    DecoderResult decoderResult = this.decoder.decode(result.Bits, null);
                    if (decoderResult != null)
                    {
                        contentString = decoderResult.Text;
                        Data dataFromJson = JsonUtility.FromJson<Data>(contentString);

                        if (dataFromJson.type == DataType.Question)
                        {
                            // TODO: check if corresponding coin is already unlocked and inform the user (Toast)
                            if (!GlobalState.UnlockedCoins.Contains(dataFromJson.id))
                            {
                                GlobalState.CurrentQuestion = dataFromJson.ToQuestion();
                                SceneManager.LoadScene("QuestionScene");
                            }
                            else
                            {
                                TextureScriptInstance.ShowToast("Already unlocked!", ToastLength);
                            }
                        }
                        else if (dataFromJson.type == DataType.Coin)
                        {
                            if (GlobalState.UnlockedCoins.Contains(dataFromJson.id))
                            {
                                if (!GlobalState.CollectedCoins.Contains(dataFromJson.id))
                                {
                                    GlobalState.CurrentCoin = dataFromJson.id;
                                    // null here, since we can not access unity api to create a game object yet.
                                    data.Add(new QRCodeData(points, null, dataFromJson.id));
                                }
                            }
                            else
                            {
                                TextureScriptInstance.ShowToast("Answer the question first!", ToastLength);
                            }
                        }
                    }
                    else
                    {
                        contentString = "ERROR";
                    }
                } else {
                    // @TODO: Apply more logic here, lol.
                    var enumerator = data.GetEnumerator();
                    enumerator.MoveNext();
                    enumerator.Current.Update(points);
                }
            }

            public override string ToString () {
                if (data.Count == 0) {
                    return "[]" + "\n" + contentString;
                }

                string result = "[" + System.Environment.NewLine;
                result = result + string.Join (",", this.data.ConvertAll (x => x.ToString ()).ToArray ());
                result = result + "]";
                result = result + "\n" + contentString;
                return result;
            }

            public void DestroyDataObject(int id)
            {
                this.data.ForEach(delegate (QRCodeData obj)
                {
                    if (obj.Id == id)
                    {
                        obj.ForceMarkAsDestroy();
                    }
                });
            }

            // Destroy the marked objects.
            // Call this only from the Unity mainthread.
            public void DestroyMarkedOnUpdate() {
                this.data.ForEach (delegate(QRCodeData obj) {
                    if (obj.IsMarkedAsDestroy()) {
                        GlobalState.CurrentCoin = -1;
                        if (obj.GetModel() == null) {
                            return;
                        }
                        Destroy (obj.GetModel());
                    }
                });

                this.data.RemoveAll (item => item.IsMarkedAsDestroy ());
            }

            // call this method in Update from Unity!!
            public void Update(Transform parent) {
                this.data.ForEach (delegate(QRCodeData code) {

                    // since we can not access Unity-API from non-main threads
                    // we have to create the game object referred by the code later in here.
                    if (code.GetModel() == null) {
                        var coin = Instantiate(Resources.Load("Coin")) as GameObject;
                        coin.transform.SetParent(parent);
                        coin.transform.localPosition = code.CenterToPlane();
                        code.SetModel(coin);
                        return;
                    }

                    code.UpdateModel();
                });
            }
        }
    }
}

//Vector3 start = new Vector3((qrCodeData.GetPoints()[0].X - CAM_WIDTH / 2) / (float)CAM_WIDTH * 1.334f * 10 * -1, 0, (qrCodeData.GetPoints()[0].Y - CAM_HEIGHT / 2) / (float)CAM_HEIGHT * 10);
//Vector3 end1 = new Vector3((qrCodeData.GetPoints()[1].X - CAM_WIDTH / 2) / (float)CAM_WIDTH * 1.334f * 10 * -1, 0, (qrCodeData.GetPoints()[1].Y - CAM_HEIGHT / 2) / (float)CAM_HEIGHT * 10);
//Vector3 end2 = new Vector3((qrCodeData.GetPoints()[2].X - CAM_WIDTH / 2) / (float)CAM_WIDTH * 1.334f * 10 * -1, 0, (qrCodeData.GetPoints()[2].Y - CAM_HEIGHT / 2) / (float)CAM_HEIGHT * 10);

//GameObject[] objects = GameObject.FindGameObjectsWithTag("c1");

//objects[0].transform.localPosition = start;
//objects[1].transform.localPosition = end1;
//objects[2].transform.localPosition = end2;
