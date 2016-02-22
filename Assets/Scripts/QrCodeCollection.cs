using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZXing.Common;
using ZXing.QrCode.Internal;
using Object = UnityEngine.Object;

namespace Assets.Scripts
{
    public class QrCodeCollection
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

				_contentString = "No QR-Code detected.";

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
                                GlobalState.Instance.CurrentQuestion = dataFromJson.id;
                                GlobalState.Instance.SceneToSwitchTo = Config.Scenes.Question;
                            }
                            else
                            {
                                CameraScript.CameraScriptInstance.SetToastToShow("Already unlocked!", CameraScript.ToastLength);
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
                                    CameraScript.CameraScriptInstance.SetToastToShow(
                                        "Tap the screen to capture the coin!", CameraScript.ToastLengthLong);
                                }
                                else
                                {
                                    CameraScript.CameraScriptInstance.SetToastToShow("Coin is already collected!", CameraScript.ToastLength);
                                }
                            }
                            else
                            {
                                CameraScript.CameraScriptInstance.SetToastToShow("Answer the question first!", CameraScript.ToastLength);
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
            result = result + String.Join(",", _data.ConvertAll(x => x.ToString()).ToArray());
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
                    Object.Destroy(obj.GetModel());
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
                    var coin = Object.Instantiate(Resources.Load("Coin")) as GameObject;
                    if (coin != null)
                    {
                        coin.transform.SetParent(parent);
                        coin.transform.localPosition = code.LocalCoinPosition();
                        code.SetModel(coin);
                    }
                    return;
                }

                code.UpdateModel();
            });
        }
    }
}