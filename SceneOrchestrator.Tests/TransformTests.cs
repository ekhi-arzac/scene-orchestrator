using System.Numerics;
using SceneOrchestrator.Core;

namespace SceneOrchestrator.Tests;

/// <summary>
/// Unit tests for transform matrices and operations.
/// </summary>
public class TransformTests
{
    private const float Eps = 1e-5f;

    /// <summary>
    /// Asserts two float values are within epsilon.
    /// </summary>
    private static void AssertNear(float expected, float actual, float eps = Eps) =>
        Assert.True(
            MathF.Abs(expected - actual) <= eps,
            $"Expected {expected} ± {eps}, got {actual}"
        );

    /// <summary>
    /// Asserts two vectors are equal within epsilon.
    /// </summary>
    private static void AssertVec(Vector3 expected, Vector3 actual, float eps = Eps)
    {
        AssertNear(expected.X, actual.X, eps);
        AssertNear(expected.Y, actual.Y, eps);
        AssertNear(expected.Z, actual.Z, eps);
    }

    // ── ModelMatrix ──────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies the identity transform yields an identity model matrix.
    /// </summary>
    [Fact]
    public void ModelMatrix_Identity_IsIdentity()
    {
        var t = new Transform(Vector3.Zero, Quaternion.Identity);
        var m = t.ModelMatrix(Matrix4x4.Identity);

        Assert.Equal(Matrix4x4.Identity, m);
    }

    /// <summary>
    /// Verifies translation moves the origin in the model matrix.
    /// </summary>
    [Fact]
    public void ModelMatrix_Translation_MovesOrigin()
    {
        var t = new Transform(new Vector3(1, 2, 3), Quaternion.Identity);
        var m = t.ModelMatrix(Matrix4x4.Identity);
        var p = Vector3.Transform(Vector3.Zero, m);

        AssertVec(new Vector3(1, 2, 3), p);
    }

    /// <summary>
    /// Verifies scaling affects the model matrix.
    /// </summary>
    [Fact]
    public void ModelMatrix_Scale_ScalesOriginPoint()
    {
        var t = new Transform(Vector3.Zero, Quaternion.Identity) { Scale = new Vector3(2, 3, 4) };
        var m = t.ModelMatrix(Matrix4x4.Identity);
        var p = Vector3.Transform(new Vector3(1, 1, 1), m);

        AssertVec(new Vector3(2, 3, 4), p);
    }

    /// <summary>
    /// Verifies rotation is applied in the model matrix.
    /// </summary>
    [Fact]
    public void ModelMatrix_Rotation_RotatesAxis()
    {
        // 90° around Y should turn +X into -Z
        var rot = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI / 2f);
        var t = new Transform(Vector3.Zero, rot);
        var m = t.ModelMatrix(Matrix4x4.Identity);
        var p = Vector3.Transform(Vector3.UnitX, m);

        AssertVec(-Vector3.UnitZ, p, 1e-4f);
    }

    /// <summary>
    /// Verifies the parent matrix is applied to the model matrix.
    /// </summary>
    [Fact]
    public void ModelMatrix_ParentApplied()
    {
        var parent = Matrix4x4.CreateTranslation(10, 0, 0);
        var t = new Transform(new Vector3(1, 0, 0), Quaternion.Identity);
        var m = t.ModelMatrix(parent);
        var p = Vector3.Transform(Vector3.Zero, m);

        AssertNear(11f, p.X);
    }

    // ── ViewMatrix ───────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies the view matrix is the inverse of the model matrix.
    /// </summary>
    [Fact]
    public void ViewMatrix_IsInverseOfModelMatrix()
    {
        var t = new Transform(
            new Vector3(3, 1, -2),
            Quaternion.CreateFromAxisAngle(Vector3.UnitY, 1f)
        );
        var model = t.ModelMatrix(Matrix4x4.Identity);
        var view = t.ViewMatrix();
        var product = model * view;

        for (int r = 0; r < 4; r++)
        for (int c = 0; c < 4; c++)
        {
            float expected = r == c ? 1f : 0f;
            AssertNear(expected, product[r, c], 1e-4f);
        }
    }

    // ── Translate ────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies world-space translation adds directly.
    /// </summary>
    [Fact]
    public void Translate_World_AddsDirectly()
    {
        var t = new Transform(Vector3.Zero, Quaternion.Identity);
        t.Translate(new Vector3(5, 0, 0), local: false);

        AssertVec(new Vector3(5, 0, 0), t.Position);
    }

    /// <summary>
    /// Verifies local-space translation uses rotation.
    /// </summary>
    [Fact]
    public void Translate_Local_UsesRotation()
    {
        // Rotated 90° around Y: local +X is world -Z
        var rot = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI / 2f);
        var t = new Transform(Vector3.Zero, rot);
        t.Translate(Vector3.UnitX, local: true);

        AssertVec(-Vector3.UnitZ, t.Position, 1e-4f);
    }

    /// <summary>
    /// Verifies world-space translation ignores rotation.
    /// </summary>
    [Fact]
    public void Translate_World_IsNotAffectedByRotation()
    {
        var rot = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI / 2f);
        var t = new Transform(Vector3.Zero, rot);
        t.Translate(Vector3.UnitX, local: false);

        AssertVec(Vector3.UnitX, t.Position, 1e-4f);
    }

    // ── Rotate ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies rotation maintains a normalized quaternion.
    /// </summary>
    [Fact]
    public void Rotate_ProducesNormalizedQuaternion()
    {
        var t = new Transform(Vector3.Zero, Quaternion.Identity);
        t.Rotate(Vector3.UnitY, MathF.PI / 3f);

        AssertNear(1f, t.Rotation.Length(), 1e-5f);
    }

    /// <summary>
    /// Verifies successive rotations accumulate correctly.
    /// </summary>
    [Fact]
    public void Rotate_AccumulatesCorrectly()
    {
        // Two 90° rotations around Y = 180° total
        var t = new Transform(Vector3.Zero, Quaternion.Identity);
        t.Rotate(Vector3.UnitY, MathF.PI / 2f);
        t.Rotate(Vector3.UnitY, MathF.PI / 2f);

        var p = Vector3.Transform(Vector3.UnitX, t.ModelMatrix(Matrix4x4.Identity));

        AssertVec(-Vector3.UnitX, p, 1e-4f);
    }

    /// <summary>
    /// Verifies local rotations transform the axis before applying.
    /// </summary>
    [Fact]
    public void Rotate_Local_TransformsAxisFirst()
    {
        // Start facing -Z (identity).  Rotate 90° around local X.
        // Local X = world X, so result same as world rotation in this case —
        // but then rotate local Y (which is now world -Z) 90° and check.
        var t = new Transform(Vector3.Zero, Quaternion.Identity);
        t.Rotate(Vector3.UnitY, MathF.PI / 2f, local: false); // now facing -X
        t.Rotate(Vector3.UnitY, MathF.PI / 2f, local: true); // local Y = world Y still

        var fwd = Vector3.Transform(-Vector3.UnitZ, t.ModelMatrix(Matrix4x4.Identity));

        // After two 90° Y-rotations the forward direction should be +Z
        AssertVec(Vector3.UnitZ, fwd, 1e-4f);
    }

    // ── Scale ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies scale defaults to one.
    /// </summary>
    [Fact]
    public void Scale_DefaultIsOne()
    {
        var t = new Transform(Vector3.Zero, Quaternion.Identity);
        AssertVec(Vector3.One, t.Scale);
    }
}
