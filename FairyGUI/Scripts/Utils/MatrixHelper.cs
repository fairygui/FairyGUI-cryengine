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

			float m00 = matrix.m00 * cosY - matrix.m10 * sinX;
			float m10 = matrix.m00 * sinY + matrix.m10 * cosX;
			float m01 = matrix.m01 * cosY - matrix.m11 * sinX;
			float m11 = matrix.m01 * sinY + matrix.m11 * cosX;
			float m02 = matrix.m02 * cosY - matrix.m12 * sinX;
			float m12 = matrix.m02 * sinY + matrix.m12 * cosX;

			matrix.m00 = m00;
			matrix.m10 = m10;
			matrix.m01 = m01;
			matrix.m11 = m11;
			matrix.m02 = m02;
			matrix.m12 = m12;
		}

		// Matrix4x4.TRS(trans, Quaternion.Euler(euler), scale)
		public static Matrix4x4 TRS(Vector3 trans, Vector3 euler, Vector3 scale)
		{
			return Translate(trans) * Rotate(euler) * Scale(scale);
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
			var mat = Matrix4x4.Identity;
			mat.m11 = cos;
			mat.m12 = -sin;
			mat.m21 = sin;
			mat.m22 = cos;
			return mat;
		}

		// Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, deg, 0), Vector3.one)
		public static Matrix4x4 RotateY(float deg)
		{
			float rad = MathHelpers.DegreesToRadians(deg);
			float sin, cos;
			MathHelpers.SinCos(rad, out sin, out cos);
			var mat = Matrix4x4.Identity;
			mat.m22 = cos;
			mat.m20 = -sin;
			mat.m02 = sin;
			mat.m00 = cos;
			return mat;
		}


		// Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, deg), Vector3.one)
		public static Matrix4x4 RotateZ(float deg)
		{
			float rad = MathHelpers.DegreesToRadians(deg);
			float sin, cos;
			MathHelpers.SinCos(rad, out sin, out cos);
			var mat = Matrix4x4.Identity;
			mat.m00 = cos;
			mat.m01 = -sin;
			mat.m10 = sin;
			mat.m11 = cos;
			return mat;
		}

		// Matrix4x4.Scale(scale)
		public static Matrix4x4 Scale(Vector3 scale)
		{
			var mat = Matrix4x4.Identity;
			mat.m00 = scale.x;
			mat.m11 = scale.y;
			mat.m22 = scale.z;
			return mat;
		}

		// Matrix4x4.TRS(vec, Quaternion.identity, Vector3.one)
		public static Matrix4x4 Translate(Vector3 vec)
		{
			var mat = Matrix4x4.Identity;
			mat.m03 = vec.x;
			mat.m13 = vec.y;
			mat.m23 = vec.z;
			return mat;
		}
	}
}
