using CryEngine;

namespace FairyGUI.Utils
{
	//ref https://github.com/n-yoda/unity-transform/tree/master/Assets/TransformMatrix
	public static class MatrixHelper
	{
		public static void Skew(ref Matrix4x4 matrix, float skewX, float skewY)
		{
			skewX = MathHelpers.DegreesToRadians(-skewX);
			skewY = MathHelpers.DegreesToRadians(-skewY);
			float sinX, cosX;
			MathHelpers.SinCos(skewX, out sinX, out cosX);
			float sinY, cosY;
			MathHelpers.SinCos(skewY, out sinY, out cosY);

			float m00 = matrix[0, 0] * cosY - matrix[1, 0] * sinX;
			float m10 = matrix[0, 0] * sinY + matrix[1, 0] * cosX;
			float m01 = matrix[0, 1] * cosY - matrix[1, 1] * sinX;
			float m11 = matrix[0, 1] * sinY + matrix[1, 1] * cosX;
			float m02 = matrix[0, 2] * cosY - matrix[1, 2] * sinX;
			float m12 = matrix[0, 2] * sinY + matrix[1, 2] * cosX;

			matrix[0, 0] = m00;
			matrix[1, 0] = m10;
			matrix[0, 1] = m01;
			matrix[1, 1] = m11;
			matrix[0, 2] = m02;
			matrix[1, 2] = m12;
		}

		// Matrix4x4.TRS(trans, Quaternion.Euler(euler), scale)
		public static Matrix4x4 TRS(Vector3 trans, Vector3 euler, Vector3 scale)
		{
			Matrix4x4 mat = Matrix4x4.Identity;
			mat.SetTranslation(trans);
			return mat * Rotate(euler) * Scale(scale);
		}

		// Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(euler), Vector3.one)
		public static Matrix4x4 Rotate(Vector3 euler)
		{
			return RotateY(euler.y) * RotateX(euler.x) * RotateZ(euler.z);
		}

		// Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(deg, 0, 0), Vector3.one)
		public static Matrix4x4 RotateX(float deg)
		{
			float rad = MathHelpers.DegreesToRadians(deg);
			float sin, cos;
			MathHelpers.SinCos(rad, out sin, out cos);
			Matrix4x4 mat = Matrix4x4.Identity;
			mat[1, 1] = cos;
			mat[1, 2] = -sin;
			mat[2, 1] = sin;
			mat[2, 2] = cos;
			return mat;
		}

		// Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, deg, 0), Vector3.one)
		public static Matrix4x4 RotateY(float deg)
		{
			float rad = MathHelpers.DegreesToRadians(deg);
			float sin, cos;
			MathHelpers.SinCos(rad, out sin, out cos);
			Matrix4x4 mat = Matrix4x4.Identity;
			mat[2, 2] = cos;
			mat[2, 0] = -sin;
			mat[0, 2] = sin;
			mat[0, 0] = cos;
			return mat;
		}


		// Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, deg), Vector3.one)
		public static Matrix4x4 RotateZ(float deg)
		{
			float rad = MathHelpers.DegreesToRadians(deg);
			float sin, cos;
			MathHelpers.SinCos(rad, out sin, out cos);
			Matrix4x4 mat = Matrix4x4.Identity;
			mat[0, 0] = cos;
			mat[0, 1] = -sin;
			mat[1, 0] = sin;
			mat[1, 1] = cos;
			return mat;
		}

		// Matrix4x4.Scale(scale)
		public static Matrix4x4 Scale(Vector3 scale)
		{
			Matrix4x4 mat = Matrix4x4.Identity;
			mat[0, 0] = scale.x;
			mat[1, 1] = scale.y;
			mat[2, 2] = scale.z;
			return mat;
		}
	}
}
