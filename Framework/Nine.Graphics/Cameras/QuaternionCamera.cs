namespace Nine.Graphics.Cameras
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Camera that can perform Quaternion rotation
    /// and look in all six direction.
    /// </summary>
    public class QuaternionCamera : ICamera
    {
        // Camera orientation, quaternion style
        private Quaternion orientation;

        // Camera position - default (0,0,0)
        private Vector3 position;

        // Field of view
        private float fieldOfView;

        // Near plane distance
        private float nearPlaneDistance;
        // Far plane distance
        private float farPlaneDistance;
        // Aspect
        private float aspect;

        // Do we need to update view matrix?
        bool needUpdateViewMatrix;

        // Do we need to update projection matrix?
        bool needUpdateProjectionMatrix;

        // Cached view matrix
        private Matrix viewMatrix;

        // Cached projection matrix
        private Matrix projectionMatrix;

        /// <summary>
        /// Creates a new instance of QuaternionCamera.
        /// </summary>
        public QuaternionCamera()
        {
            orientation = Quaternion.Identity;
            position = Vector3.Zero;

            this.nearPlaneDistance = 100.0f;
            this.farPlaneDistance = 100000.0f;
            this.aspect = 1.33333333333333f;
            this.fieldOfView = MathHelper.Pi / 4.0f;

            needUpdateViewMatrix = true;
            needUpdateProjectionMatrix = true;

            viewMatrix = projectionMatrix = Matrix.Identity;
        }

        /// <summary>
        /// Moves the camera's position by the vector offset provided along world axes.
        /// </summary>
        /// <param name="translate"></param>
        public void Move(Vector3 translate)
        {
            this.position = this.position + translate;

            // Trigger to update view matrix.
            this.needUpdateViewMatrix = true;
        }

        /// <summary>
        /// Moves the camera's position by the vector offset 
        /// provided along it's own axes (relative to orientation).
        /// </summary>
        /// <param name="translate"></param>
        public void MoveRelative(Vector3 translate)
        {
            // Transform the axes of the relative vector by camera's local axes
            Vector3 trans = MultiplyQuaternion(orientation, translate);

            this.position = this.position + trans;

            // Trigger to update view matrix.
            this.needUpdateViewMatrix = true;
        }

        /// <summary>
        /// Rolls the camera anticlockwise, around its local z axis.
        /// </summary>
        /// <param name="angle"></param>
        public void Roll(float angle)
        {
            // Rotate around local Z axis
            Vector3 zAxis = MultiplyQuaternion(this.orientation, Vector3.UnitZ);
            Rotate(zAxis, angle);
        }

        /// <summary>
        /// Rotates the camera anticlockwise around it's local y axis.
        /// </summary>
        /// <param name="angle"></param>
        public void Yaw(float angle)
        {
            Vector3 yAxis;

            // Rotate around local Y axis
            yAxis = MultiplyQuaternion(this.orientation, Vector3.UnitY);

            Rotate(yAxis, angle);
        }

        /// <summary>
        /// Pitches the camera up/down anticlockwise around it's local z axis.
        /// </summary>
        /// <param name="angle"></param>
        public void Pitch(float angle)
        {
            // Rotate around local X axis
            Vector3 xAxis = MultiplyQuaternion(this.orientation, Vector3.UnitX);
            Rotate(xAxis, angle);
        }

        /// <summary>
        /// Rotate the camera around an arbitrary axis.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="angle"></param>
        public void Rotate(Vector3 axis, float angle)
        {
            Quaternion q = Quaternion.CreateFromAxisAngle(axis, angle);
            Rotate(q);
        }

        /// <summary>
        /// Rotate the camera around an arbitrary axis using a Quaternion.

        /// </summary>
        /// <param name="q"></param>
        public void Rotate(Quaternion q)
        {
            // Normalize the quat to avoid cumulative problems with precision
            Quaternion qnorm = q;
            qnorm.Normalize();
            this.orientation = qnorm * this.orientation;

            // Trigger to update view matrix.
            this.needUpdateViewMatrix = true;
        }

        /// <summary>
        /// Performs update of ViewMatrix.
        /// </summary>
        private void UpdateViewMatrix()
        {
            if (needUpdateViewMatrix)
            {
                viewMatrix = MakeViewMatrix(position, orientation);
            }
        }

        /// <summary>
        /// Performs update of ProjectionMatrix.
        /// </summary>
        private void UpdateProjectionMatrix()
        {
            if (needUpdateProjectionMatrix)
            {
                projectionMatrix = Matrix.CreatePerspectiveFieldOfView(this.fieldOfView,
                    this.aspect, this.nearPlaneDistance, this.farPlaneDistance);
            }
        }

        #region MakeViewMatrix
        /// <summary>
        /// Creates view matrix from position and orientation
        /// </summary>
        private static Matrix MakeViewMatrix(Vector3 position, Quaternion orientation)
        {
            Matrix viewMatrix = Matrix.CreateTranslation(-position) * Matrix.CreateFromQuaternion(Quaternion.Inverse(orientation));

            return viewMatrix;
        }
        #endregion

        /// <summary>
        /// Multiply quaterion by Vector3
        /// </summary>
        /// <param name="leftQuaterion"></param>
        /// <param name="rightVector"></param>
        /// <returns></returns>
        private static Vector3 MultiplyQuaternion(Quaternion leftQuaterion, Vector3 rightVector)
        {
            // nVidia SDK implementation
            Vector3 uv, uuv;
            Vector3 qvec = new Vector3(leftQuaterion.X, leftQuaterion.Y, leftQuaterion.Z);
            uv = Vector3.Cross(qvec, rightVector);
            uuv = Vector3.Cross(qvec, uv);
            uv *= (2.0f * leftQuaterion.W);
            uuv *= 2.0f;
            
            return rightVector + uv + uuv;

        }

        /// <summary>
        /// Gets the optional viewport of this cameara.
        /// </summary>
        public Viewport? Viewport { get; set; }

        /// <summary>
        /// Gets or Sets camera position.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;

                // Trigger to update view matrix.
                needUpdateViewMatrix = true;
            }
        }

        /// <summary>
        /// Gets or Sets camera orientation.
        /// </summary>
        public Quaternion Orientation
        {
            get
            {
                return orientation;
            }
            set
            {
                orientation = value;

                // Trigger to update view matrix.
                needUpdateViewMatrix = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float NearPlane
        {
            get
            {
                return nearPlaneDistance;
            }
            set
            {
                nearPlaneDistance = value;

                // Trigger to update projection matrix.
                needUpdateProjectionMatrix = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float FarPlane
        {
            get
            {
                return farPlaneDistance;
            }
            set
            {
                farPlaneDistance = value;

                // Trigger to update projection matrix.
                needUpdateProjectionMatrix = true;
            }
        }


        /// <summary>
        /// Gets or Sets camera aspect ratio.
        /// </summary>
        public float AspectRatio
        {
            get
            {
                return aspect;
            }
            set
            {
                aspect = value;

                // Trigger to update projection matrix.
                needUpdateProjectionMatrix = true;
            }
        }

        /// <summary>
        /// Gets or Sets camera field of view.
        /// <remarks>Value are expressed in radians</remarks>
        /// </summary>
        public float FieldOfView
        {
            get
            {
                return fieldOfView;
            }
            set
            {
                fieldOfView = value;

                // Trigger to update projection matrix.
                needUpdateProjectionMatrix = true;
            }
        }

        #region ICamera Members

        public Matrix View
        {
            get
            {
                UpdateViewMatrix();
                return viewMatrix;
            }
        }

        public Matrix Projection
        {
            get
            {
                UpdateProjectionMatrix();
                return projectionMatrix;
            }
        }

        #endregion
    }
}
