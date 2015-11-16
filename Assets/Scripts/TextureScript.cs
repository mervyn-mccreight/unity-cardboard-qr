using System.Threading;
using UnityEngine;
using ZXing;
using ZXing.QrCode;
using UnityEngine.UI;
using ZXing.QrCode.Internal;
using ZXing.Common;

public class TextureScript : MonoBehaviour {
	public Text uiText;

	private WebCamTexture webcamTexture;
    private WebCamDevice backFacing;
    private Thread qrCodeThread;
    private bool runThread = true;
	private QRCodeData qrCodeData;

    // Use this for initialization
    void Start () {
        Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.Log("access to webcam granted!");
            Debug.Log("#WebCamDevices: " + WebCamTexture.devices.GetLength(0).ToString());

            foreach (WebCamDevice device in WebCamTexture.devices)
            {
                Debug.Log("WebCamDevice: " + device.name);
                Debug.Log("FrontFacing? " + device.isFrontFacing);
                if (!device.isFrontFacing)
                {
                    backFacing = device;
                }
            }
            webcamTexture = new WebCamTexture(backFacing.name, 1024, 768);
   
            GetComponent<Renderer>().material.SetTexture("_MainTex", webcamTexture);
            webcamTexture.Play();

            qrCodeThread = new Thread(DecodeQR);
            qrCodeThread.Start();

        } else
        {
            Debug.LogError("No User Authorization for Camera Device.");
        }
    }

    void DecodeQR()
    {
        while (runThread)
        {

			LuminanceSource lum = new Color32LuminanceSource(webcamTexture.GetPixels32(), webcamTexture.width, webcamTexture.height);
			HybridBinarizer bin = new HybridBinarizer(lum);
			BinaryBitmap binBip = new BinaryBitmap(bin);
			BitMatrix matrix = binBip.BlackMatrix;
			Detector detector = new Detector(matrix);

			DetectorResult result = detector.detect();
            if (result != null)
            {
                ResultPoint[] points = result.Points;

				if (qrCodeData == null) {
					qrCodeData = new QRCodeData(points, null);
				} else {
					qrCodeData.Update(points);
				}

				uiText.text = qrCodeData.ToString();

                Vector3 start = new Vector3((points[0].X - webcamTexture.width / 2) / (float)webcamTexture.width * 1.334f * 10 * -1, 0, (points[0].Y - webcamTexture.height / 2) / (float)webcamTexture.height * 10);
                Vector3 end1 = new Vector3((points[1].X - webcamTexture.width / 2) / (float)webcamTexture.width * 1.334f * 10 * -1, 0, (points[1].Y - webcamTexture.height / 2) / (float)webcamTexture.height * 10);
                Vector3 end2 = new Vector3((points[2].X - webcamTexture.width / 2) / (float)webcamTexture.width * 1.334f * 10 * -1, 0, (points[2].Y - webcamTexture.height / 2) / (float)webcamTexture.height * 10);
                Debug.Log(start);
                Debug.Log(end1);
                Debug.Log(end2);

                GameObject[] objects = GameObject.FindGameObjectsWithTag("c1");

                objects[0].transform.localPosition = start;
                objects[1].transform.localPosition = end1;
                objects[2].transform.localPosition = end2;

                Debug.Log(result.ToString());
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
    }

    // Sent to all game objects before the application is quit.
    void OnApplicationQuit()
    {
        runThread = false;
    }

    // This function is called when the MonoBehaviour will be destroyed.
    void OnDestroy()
    {
        qrCodeThread.Abort();
        webcamTexture.Stop();
    }

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

		public override string ToString ()
		{
			string result = "";
			foreach (ResultPoint resultPoint in this.resultPoints) {
				result = result + string.Format("({0}, {1})", resultPoint.X, resultPoint.Y) + System.Environment.NewLine;
			}
			return result;
		}
	}
}
