using System;
using System.Linq;
using UnityEngine;
using ZXing;
using Object = UnityEngine.Object;

namespace Assets.Scripts
{
    /// <summary>
    /// Handles the model data corresponding to a QR-code.
    /// </summary>
    public abstract class QrCodeData
    {
        private ResultPoint[] _target;
        private GameObject _model;
        public int Id { get; private set; }
        private DateTime _time;
        private DateTime _destroyTime = DateTime.MinValue;
        private bool _forceDestroy;

        /// <summary>
        /// Time the model stays visible after the QR-code is no longer detected.
        /// This is needed to prevent flickering of the model when the QR-code is lost for short periods of time, e.g. when the camera re-focuses.
        /// </summary>
        private const float KeepAliveTime = 0.5f;

        /// <summary>
        /// Speed in world units per second that the model moves in the direction of the QR-code.
        /// The model movement is interpolated to prevent it from constantly jumping across the screen when the camera moves.
        /// </summary>
        private const float InterpolationSpeed = 2f;

        /// <summary>
        /// Constructs a QrCodeData object.
        /// </summary>
        /// <param name="resultPoints">Result points detected by ZXing</param>
        /// <param name="id">Id of QR-code</param>
        public QrCodeData(ResultPoint[] resultPoints, int id)
        {
            _target = resultPoints;
            Id = id;
            _time = DateTime.UtcNow;
        }

        /// <summary>
        /// Update the position of the object through new <see cref="ResultPoint"/>s. Also resets the timers used to detroy the object.
        /// </summary>
        /// <param name="newResultPoints">Newly detected QR-code position</param>
        public void Update(ResultPoint[] newResultPoints)
        {
            _target = newResultPoints;
            _time = DateTime.UtcNow;
            _destroyTime = DateTime.MinValue;
        }

        /// <summary>
        /// Creates a model based on the <see cref="DataType"/>.
        /// </summary>
        /// <param name="parent"></param>
        public abstract void CreateModel(Transform parent);

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

        /// <summary>
        /// Marks the object as to be destroyed.
        /// It will be destroyed after <see cref="KeepAliveTime"/> seconds have passed without it being updated.
        /// </summary>
        public void MarkAsDestroy()
        {
            if (_destroyTime == DateTime.MinValue)
            {
                _destroyTime = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Determines whether this object should be destroyed at the moment of this method being called.
        /// That is the case if it is either marked to be destroyed no matter what (see <see cref="ForceMarkAsDestroy"/>),
        /// or it has been marked and the <see cref="KeepAliveTime"/> has expired.
        /// </summary>
        /// <returns>True if the object should be destroyed, false otherwise.</returns>
        public bool ShouldBeDestroyed()
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
            var result = _target.Aggregate("", (accumulator, resultPoint) => accumulator + String.Format("({0}, {1})", resultPoint.X, resultPoint.Y) + Environment.NewLine);

            var modelString = "none";
            if (_model != null)
            {
                modelString = _model.ToString();
            }

            return result + "Type =" + modelString + Environment.NewLine;
        }

        /// <summary>
        /// Calculates a diagonal across the QR-code in 2D image pixel coordinates.
        /// </summary>
        /// <returns>Diagonal vector</returns>
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

        /// <summary>
        /// Calculates the models position relative to its parent (the camera texture plane).
        /// </summary>
        /// <returns>Local position vector</returns>
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


            // Note: moving objects around in the scene WILL BREAK THIS CALCULATION!

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

        /// <summary>
        /// Scales the model to roughly match the size of the QR-code on the plane.
        /// </summary>
        /// <returns></returns>
        private Vector3 ScaleFactor()
        {
            // Empirical values.
            return new Vector3(Diagonal().magnitude/200f, Diagonal().magnitude/200f, Diagonal().magnitude/200f);
        }

        /// <summary>
        /// Update the model position, scale and rotation. Called from <see cref="QrCodeCollection.Update"/>.
        /// </summary>
        public void UpdateModel()
        {
            GetModel().transform.localPosition = Interpolate(GetModel().transform.localPosition, LocalCoinPosition());
            GetModel().transform.localScale = Interpolate(GetModel().transform.localScale, ScaleFactor());
            GetModel().transform.Rotate(Vector3.forward*Time.smoothDeltaTime*100f);
        }

        private Vector3 Interpolate(Vector3 from, Vector3 to)
        {
            var traveledDistance = (float) DateTime.UtcNow.Subtract(_time).TotalSeconds*InterpolationSpeed;
            var totalDistance = Vector3.Distance(@from, to);

            return Vector3.Lerp(@from, to, traveledDistance/totalDistance);
        }
    }
}