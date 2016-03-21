using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZXing.Common;
using ZXing.QrCode.Internal;
using Object = UnityEngine.Object;

namespace Assets.Scripts
{
    /// <summary>
    /// Handles the data of multiple QR codes detected in an image.
    /// </summary>
    public class QrCodeCollection
    {
        private readonly List<QrCodeData> _data = new List<QrCodeData>();
        private readonly Decoder _decoder = new Decoder();

        // contains QR-code content, or otherwise a human-readable message. Useful for debugging.
        private string _contentString = "ERROR";

        /// <summary>
        /// Update the collection with newly detected QR-codes. Called by the QR-code detection thread (<see cref="CameraScript._qrCodeThread"/>).
        /// </summary>
        /// <param name="result">QR-code detection result</param>
        public void UpdateData(DetectorResult result)
        {
            if (result == null)
            {
                // If there is no qr-code in the image, clear the area.
                _data.ForEach(delegate(QrCodeData obj)
                {
                    // mark destroy all models attached to qrcodedata before cleaning the list.
                    obj.MarkAsDestroy();
                });

                _contentString = "No QR-Code detected.";

                return;
            }

            var points = result.Points;

            // If no QR-codes are visible so far...
            if (_data.Count == 0)
            {
                // ... decode the new one ...
                var decoderResult = _decoder.decode(result.Bits, null);
                if (decoderResult != null)
                {
                    // ... and if that worked, handle it based on its type:
                    _contentString = decoderResult.Text;
                    var dataFromJson = JsonUtility.FromJson<Data>(_contentString);

                    switch (dataFromJson.type)
                    {
                        case DataType.Question:
                            if (!GlobalState.Instance.IsCoinUnlocked(dataFromJson.id))
                            {
                                // If it's a question that has not been answered yet, switch to QuestionScene.
                                GlobalState.Instance.CurrentQuestion = dataFromJson.id;
                                GlobalState.Instance.SceneToSwitchTo = Config.Scenes.Question;
                            }
                            else
                            {
                                // Otherwise show a toas to the user.
                                CameraScript.CameraScriptInstance.SetToastToShow(
                                    StringResources.QuestionAlreadyAnsweredToastMessage, CameraScript.ToastLength);
                            }
                            break;
                        case DataType.Coin:
                            if (GlobalState.Instance.IsCoinUnlocked(dataFromJson.id))
                            {
                                if (!GlobalState.Instance.IsCoinCollected(dataFromJson.id))
                                {
                                    // If it's a coin that has not been collected yet, add a new data object to the collection to track it.
                                    GlobalState.Instance.CurrentCoin = dataFromJson.id;
                                    // null here, since we can not access unity api to create a game object yet.
                                    _data.Add(new QrCodeData(points, dataFromJson.id, dataFromJson.type));
                                    CameraScript.CameraScriptInstance.SetToastToShow(
                                        StringResources.TapCoinToCollectToastMessage, CameraScript.ToastLengthLong);
                                }
                                else
                                {
                                    CameraScript.CameraScriptInstance.SetToastToShow(
                                        StringResources.CoinAlreadyCollectedToastMessage, CameraScript.ToastLength);
                                }
                            }
                            else
                            {
                                CameraScript.CameraScriptInstance.SetToastToShow(
                                    StringResources.AnswerQuestionFirstToastMessage, CameraScript.ToastLength);
                            }
                            break;
                        case DataType.Particle:
                            // If it's a particle, add a new data object to track it.
                            _data.Add(new QrCodeData(points, dataFromJson.id, dataFromJson.type));
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
                // If there is already an object on screen, update it with the new decoder result.
                // Note: this currently only supports a single object on screen at any time.
                var enumerator = _data.GetEnumerator();
                enumerator.MoveNext();
                if (enumerator.Current != null)
                {
                    enumerator.Current.Update(points);
                }
            }
        }

        // Useful for debugging purposes.
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

        /// <summary>
        /// Destroy an object forcefully.
        /// Unlike <see cref="QrCodeData.MarkAsDestroy"/>, which only destroys an object after some time has passed to allow for re-recognition of the QR-code,
        /// this destroys the object instantly.
        /// It is used to hide the coin when the user collects it.
        /// </summary>
        /// <param name="id">Id of the object to be destroyed.</param>
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
        
        /// <summary>
        /// Destroy the marked objects.
        /// Call this only from the Unity mainthread.
        /// </summary>
        public void DestroyMarkedOnUpdate()
        {
            _data.ForEach(delegate(QrCodeData obj)
            {
                if (obj.ShouldBeDestroyed())
                {
                    GlobalState.Instance.CurrentCoin = -1;
                    if (obj.GetModel() == null)
                    {
                        return;
                    }
                    Object.Destroy(obj.GetModel());
                }
            });

            _data.RemoveAll(item => item.ShouldBeDestroyed());
        }

        /// <summary>
        /// Update the models corresponding to the QR-code data.
        /// Call from <see cref="CameraScript.Update"/>.
        /// </summary>
        /// <param name="parent">Parent object for models. Needed for relative positioning.</param>
        public void Update(Transform parent)
        {
            _data.ForEach(delegate(QrCodeData code)
            {
                // since we can not access Unity-API from non-main threads
                // we have to create the game object referred by the code later in here.
                if (code.GetModel() == null)
                {
                    code.CreateModel(parent);
                    return;
                }

                code.UpdateModel();
            });
        }
    }
}