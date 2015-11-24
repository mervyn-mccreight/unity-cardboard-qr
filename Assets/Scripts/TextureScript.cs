using System.Threading;
using UnityEngine;
using ZXing;
using ZXing.QrCode;
using UnityEngine.UI;
using ZXing.QrCode.Internal;
using ZXing.Common;
using System.Collections.Generic;

public class TextureScript : MonoBehaviour {
	public Text uiText;

	private WebCamTexture webcamTexture;
    private WebCamDevice backFacing;
    private Thread qrCodeThread;
    private bool runThread = true;
	private QRCodeCollection qrCodeCollection = new QRCodeCollection();

	private Color32[] pixels = null;
	private static int CAM_WIDTH = 1024;
	private static int CAM_HEIGHT = 768;

    // Use this for initialization
    void Start () {
        Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam)) {
            Debug.Log("access to webcam granted!");
            Debug.Log("#WebCamDevices: " + WebCamTexture.devices.GetLength(0).ToString());

            foreach (WebCamDevice device in WebCamTexture.devices) {
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
			if (pixels != null) {

				LuminanceSource lum = new Color32LuminanceSource(pixels, CAM_WIDTH, CAM_HEIGHT);
				HybridBinarizer bin = new HybridBinarizer(lum);
				BinaryBitmap binBip = new BinaryBitmap(bin);
				BitMatrix matrix = binBip.BlackMatrix;
				Detector detector = new Detector(matrix);

				DetectorResult result = detector.detect();
				qrCodeCollection.Update(result);
			}
        }
    }
	
	// Update is called once per frame
	void Update () {
		// first draw upon the old data.
		uiText.text = qrCodeCollection.ToString();
		
		//Vector3 start = new Vector3((qrCodeData.GetPoints()[0].X - CAM_WIDTH / 2) / (float)CAM_WIDTH * 1.334f * 10 * -1, 0, (qrCodeData.GetPoints()[0].Y - CAM_HEIGHT / 2) / (float)CAM_HEIGHT * 10);
		//Vector3 end1 = new Vector3((qrCodeData.GetPoints()[1].X - CAM_WIDTH / 2) / (float)CAM_WIDTH * 1.334f * 10 * -1, 0, (qrCodeData.GetPoints()[1].Y - CAM_HEIGHT / 2) / (float)CAM_HEIGHT * 10);
		//Vector3 end2 = new Vector3((qrCodeData.GetPoints()[2].X - CAM_WIDTH / 2) / (float)CAM_WIDTH * 1.334f * 10 * -1, 0, (qrCodeData.GetPoints()[2].Y - CAM_HEIGHT / 2) / (float)CAM_HEIGHT * 10);
		
		//GameObject[] objects = GameObject.FindGameObjectsWithTag("c1");
		
		//objects[0].transform.localPosition = start;
		//objects[1].transform.localPosition = end1;
		//objects[2].transform.localPosition = end2;

		// then fetch new data for new calculations.
		pixels = webcamTexture.GetPixels32 ();
    }

    // Sent to all game objects before the application is quit.
    void OnApplicationQuit() {
        runThread = false;
    }

    // This function is called when the MonoBehaviour will be destroyed.
    void OnDestroy() {
        qrCodeThread.Abort();
        webcamTexture.Stop();
    }

	// Class for Mapping QR-Code Data to a Unity GameObject.
	private class QRCodeData {
		private ResultPoint[] resultPoints;
		private GameObject model;

		public QRCodeData(ResultPoint[] resultPoints, GameObject model) {
			this.resultPoints = resultPoints;
			this.model = model;
		}

		public void Update(ResultPoint[] newResultPoints) {
			this.resultPoints = newResultPoints;
		}

		public ResultPoint[] GetPoints() {
			return this.resultPoints;
		}

		public override string ToString () {
			string result = "";
			foreach (ResultPoint resultPoint in this.resultPoints) {
				result = result + string.Format("({0}, {1})", resultPoint.X, resultPoint.Y) + System.Environment.NewLine;
			}
			return result;
		}
	}

	private class QRCodeCollection {
		private List<QRCodeData> data = new List<QRCodeData>();

		public void Update(DetectorResult result) {
			if (result == null) {
				// if there is no qr-code in the image, clear the area.
				data.Clear();
				return;
			}

			ResultPoint[] points = result.Points;

			if (data.Count == 0) {
				data.Add(new QRCodeData(points, null));
			} else {
				// @TODO: Apply more logic here, lol.
				var enumerator = data.GetEnumerator();
				enumerator.MoveNext();
				enumerator.Current.Update(points);
			}
		}

		public override string ToString () {
			if (data.Count == 0) {
				return "[]";
			}

			string result = "[" + System.Environment.NewLine;
			result = result + string.Join (",", this.data.ConvertAll (x => x.ToString ()).ToArray ());
			result = result + "]";
			return result;
		}
	}
}
