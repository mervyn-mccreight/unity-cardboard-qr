using System;
using System.Linq;
using UnityEngine;
using ZXing;

namespace Assets.Scripts
{
    public class QrCodeData
    {
        private ResultPoint[] _target;
        private GameObject _model;
        public int Id { get; private set; }
        private DateTime _time;
        private DateTime _destroyTime = DateTime.MinValue;
        private bool _forceDestroy;

        private const float KeepAliveTime = 0.5f;
        private const float InterpolationSpeed = 2f;

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
                    accumulator + String.Format("({0}, {1})", resultPoint.X, resultPoint.Y) + Environment.NewLine);

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

            return new Vector3((position.x - (float) Config.CamWidth/2)/Config.CamWidth*1.334f*10*-1, 2,
                (position.y - (float) Config.CamHeight/2)/Config.CamHeight*10);
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
            var totalDistance = Vector3.Distance(@from, to);

            return Vector3.Lerp(@from, to, traveledDistance/totalDistance);
        }
    }
}