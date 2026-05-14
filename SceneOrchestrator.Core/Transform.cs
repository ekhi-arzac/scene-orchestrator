using System.Numerics;

class Transform
{
    public Matrix4x4 LocalMatrix { get; private set; }
    public Vector4 Position => LocalMatrix.W;

}
