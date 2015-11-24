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
				qrCodeCollection.UpdateData(result);
			}
        }
    }
	
	// Update is called once per frame
	void Update () {
		// destroy data which is marked as to be destroyed from the thread.
		this.qrCodeCollection.DestroyMarkedOnUpdate ();

		// first draw upon the old data.
		uiText.text = qrCodeCollection.ToString();

		// update the qr code object drawings.
		this.qrCodeCollection.Update (transform);

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
		private bool destroy = false;

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

		public void SetModel(GameObject model) {
			this.model = model;
		}

		public GameObject GetModel() {
			return this.model;
		}

		public void MarkAsDestroy() {
			this.destroy = true;
		}

		public bool IsMarkedAsDestroy() {
			return this.destroy;
		}

		public override string ToString () {
			string result = "";
			foreach (ResultPoint resultPoint in this.resultPoints) {
				result = result + string.Format("({0}, {1})", resultPoint.X, resultPoint.Y) + System.Environment.NewLine;
			}

			
			string modelString = "none";
			if (this.model != null) {
				modelString = this.model.ToString();
			}

			result = result + "Type =" + modelString + System.Environment.NewLine;
			return result;
		}
	}

	private class QRCodeCollection {
		private List<QRCodeData> data = new List<QRCodeData>();

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
				// null here, since we can not access unity api to create a game object yet.
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

		public void DestroyMarkedOnUpdate() {
			this.data.ForEach (delegate(QRCodeData obj) {
				if (obj.IsMarkedAsDestroy()) {
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
					var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
					code.SetModel(cube);
				}

				code.GetModel().transform.parent = parent;

				// TODO: the whole center of a qr code calculation is fucked up, lol.
				//var p1 = new Vector3(code.GetPoints()[0].X, code.GetPoints()[0].Y);
				//var p2 = new Vector3(code.GetPoints()[1].X, code.GetPoints()[1].Y);
				//var p3 = new Vector3(code.GetPoints()[2].X, code.GetPoints()[2].Y);

				//var ab = p1 - p2;
				//var ac = p1 - p3;
				//var bc = p2 - p3;

				//Vector3 max = new Vector3(0, 0);

				//// find the longest line.
				//if (ab.magnitude > ac.magnitude) {
				//	max = ab;
				//} else {
				//	max = ac;
				//}

				//if (bc.magnitude > max.magnitude) {
				//	max = bc;
				//}

				//Vector3 position = new Vector3(0, 0);
				//if (max == ab || max == ac) {
				//	position = p1 + (max / 2);
				//} else {
				//	position = p2 + (max / 2);
				//}

				//TODO: use center instead of just one point for the position.
				Vector3 position = new Vector3((code.GetPoints()[0].X - CAM_WIDTH / 2) / (float)CAM_WIDTH * 1.334f * 10 * -1, 0, (code.GetPoints()[0].Y - CAM_HEIGHT / 2) / (float)CAM_HEIGHT * 10);
				//code.GetModel().gameObject.transform.localPosition = position;
				code.GetModel().transform.localPosition = position;
			});
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
