using Rays._3D;
using System.Numerics;

namespace Rays.Scenes;

/// <summary>
/// Original implementation from https://github.com/giawa/opengl4csharp/blob/dotnetcore/OpenGL/Math/Frustum.cs
/// </summary>
public readonly struct Frustum
{
    private readonly Plane[] _planes;

    /// <summary>
    /// Builds the Planes so that they make up the left, right, up, down, front and back of the Frustum.
    /// </summary>
    public Frustum(Matrix4x4 projectionMatrix, Matrix4x4 modelViewMatrix)
    {
        _planes = new Plane[6];

        Matrix4x4 clipMatrix = modelViewMatrix * projectionMatrix;
        _planes[0] = new Plane(clipMatrix.M44 - clipMatrix.M41, new Vector3(clipMatrix.M14 - clipMatrix.M11, clipMatrix.M24 - clipMatrix.M21, clipMatrix.M34 - clipMatrix.M31));
        _planes[1] = new Plane(clipMatrix.M44 + clipMatrix.M41, new Vector3(clipMatrix.M14 + clipMatrix.M11, clipMatrix.M24 + clipMatrix.M21, clipMatrix.M34 + clipMatrix.M31));
        _planes[2] = new Plane(clipMatrix.M44 + clipMatrix.M42, new Vector3(clipMatrix.M14 + clipMatrix.M12, clipMatrix.M24 + clipMatrix.M22, clipMatrix.M34 + clipMatrix.M32));
        _planes[3] = new Plane(clipMatrix.M44 - clipMatrix.M42, new Vector3(clipMatrix.M14 - clipMatrix.M12, clipMatrix.M24 - clipMatrix.M22, clipMatrix.M34 - clipMatrix.M32));
        _planes[4] = new Plane(clipMatrix.M44 - clipMatrix.M43, new Vector3(clipMatrix.M14 - clipMatrix.M13, clipMatrix.M24 - clipMatrix.M23, clipMatrix.M34 - clipMatrix.M33));
        _planes[5] = new Plane(clipMatrix.M44 + clipMatrix.M43, new Vector3(clipMatrix.M14 + clipMatrix.M13, clipMatrix.M24 + clipMatrix.M23, clipMatrix.M34 + clipMatrix.M33));

        for (int i = 0; i < 6; i++)
        {
            float length = _planes[i].Normal.Length();
            _planes[i] = new Plane(_planes[i].D / length, _planes[i].Normal / length);
        }
    }

    private bool Intersects(Vector4 center, Vector4 size)
    {
        for (int i = 0; i < 6; i++)
        {
            Plane p = _planes[i];
            Vector4 planeNormal = p.Normal.ToZeroExtendedVector4();

            float d = Vector4.Dot(center, planeNormal);
            float r = Vector4.Dot(size, Vector4.Abs(planeNormal));
            float dpr = d + r;

            if (dpr < -p.D)
            {
                return false;
            }
        }
        return true;
    }

    private readonly record struct Plane(float D, Vector3 Normal);
}
