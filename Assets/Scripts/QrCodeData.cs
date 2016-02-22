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

        public Vector3 LocalCoinPosition()
        {
            // the three points of interest in a qr code
            var p1 = new Vector3(_target[0].X, _target[0].Y);
            var p2 = new Vector3(_target[1].X, _target[1].Y);
            var p3 = new Vector3(_target[2].X, _target[2].Y);

            // distance between the three points
            var ab = p2 - p1;
            var ac = p3 - p1;

            // determine which distance is the diagonal of the qr code
            // position equals the midway point of the diagonal
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
            
            // pixel coordiate range converted to plane coordinate system range
            var xTransformed = position.x*GlobalState.Instance.PlaneWidth/GlobalState.Instance.CamWidth;
            var zTransformed = position.y*GlobalState.Instance.PlaneHeight/GlobalState.Instance.CamHeight;

            // align with new coordinate system origin
            xTransformed -= GlobalState.Instance.PlaneWidth/2;
            zTransformed -= GlobalState.Instance.PlaneHeight/2;

            // Calculated relative to camera texture plane:
            // imaginary plane that the coin will lie on, so that it is in front of the camera texture plane
            var plane = new Plane(Vector3.up, new Vector3(0, 2, 0));
            // camera position
            var cam = new Vector3(0, 6, 0);
            // QR code center point on camera texture plane
            var targetPoint = new Vector3(xTransformed*-1, 0, zTransformed);
            // ray from camera to QR code center
            var camToTarget = targetPoint - cam;
            // calculates length of normalized ray where it intersects the imaginary plane
            // (which is the correct position of the coin when taking perspective into account)
            float enter;
            plane.Raycast(new Ray(cam, camToTarget), out enter);
            
            // origin of ray + normalized direction * intersection length
            return cam + (camToTarget.normalized*enter);
        }

        private Vector3 ScaleFactor()
        {
            return new Vector3(Diagonal().magnitude/100f, Diagonal().magnitude/100f*0.05f, Diagonal().magnitude/100f);
        }

        public void UpdateModel()
        {
            GetModel().transform.localPosition = Interpolate(GetModel().transform.localPosition, LocalCoinPosition());
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