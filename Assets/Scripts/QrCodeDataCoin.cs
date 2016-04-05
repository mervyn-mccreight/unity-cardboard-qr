using UnityEngine;
using ZXing;
using Object = UnityEngine.Object;

namespace Assets.Scripts
{
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
