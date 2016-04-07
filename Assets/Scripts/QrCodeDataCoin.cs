using UnityEngine;
using ZXing;
using Object = UnityEngine.Object;

namespace Assets.Scripts
{
    /// <summary>
    /// Handles the model data for coin QR codes.
    /// </summary>
    public class QrCodeDataCoin : QrCodeData
    {
        public QrCodeDataCoin(ResultPoint[] resultPoints, int id) : base(resultPoints, id)
        {
        }

        public override void CreateModel(Transform parent)
        {
            GameObject model = Object.Instantiate(Resources.Load("Coin")) as GameObject;

            if (model != null)
            {
                model.transform.SetParent(parent);
                model.transform.localPosition = LocalCoinPosition();
                SetModel(model);
            }
        }
    }
}
