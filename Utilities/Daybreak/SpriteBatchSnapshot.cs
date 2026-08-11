#nullable enable

using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityMod.Utilities.Daybreak;

/// <summary>
///     A "snapshot" of the current state of a <see cref="SpriteBatch" />.
///     <br />
///     These values may be manipulated freely.
/// </summary>
/// <remarks>
///     This API exists for making preservation of a <see cref="SpriteBatch" />'s
///     state trivial.
///     <br />
///     The act of taking a snapshot through this object's constructor is pure
///     (that is, it has no side effects).  It will not mutate the state of the
///     <see cref="SpriteBatch" /> being analyzed.  If you intend to modify the
///     <see cref="SpriteBatch" />, use the APIs provided in
///     <see cref="SpriteBatchSnapshotExtensions" />.
/// </remarks>
internal struct SpriteBatchSnapshot
{
    /// <summary>
    ///     The sort mode.
    /// </summary>
    public SpriteSortMode SortMode { get; set; }

    /// <summary>
    ///     The blend state.
    /// </summary>
    public BlendState BlendState { get; set; }

    /// <summary>
    ///     The sampler state.
    /// </summary>
    public SamplerState SamplerState { get; set; }

    /// <summary>
    ///     The depth stencil state.
    /// </summary>
    public DepthStencilState DepthStencilState { get; set; }

    /// <summary>
    ///     The rasterizer state.
    /// </summary>
    public RasterizerState RasterizerState { get; set; }

    /// <summary>
    ///     The custom effect, if applicable.
    /// </summary>
    public Effect? CustomEffect { get; set; }

    /// <summary>
    ///     The transformation matrix.
    /// </summary>
    public Matrix TransformMatrix { get; set; }

    /// <summary>
    ///     Creates a new <see cref="SpriteBatch"/> snapshot from raw
    ///     parameters.
    /// </summary>
    public SpriteBatchSnapshot(
        SpriteSortMode sortMode,
        BlendState blendState,
        SamplerState samplerState,
        DepthStencilState depthStencilState,
        RasterizerState rasterizerState,
        Effect? customEffect,
        Matrix transformMatrix
    )
    {
        SortMode = sortMode;
        BlendState = blendState;
        SamplerState = samplerState;
        DepthStencilState = depthStencilState;
        RasterizerState = rasterizerState;
        CustomEffect = customEffect;
        TransformMatrix = transformMatrix;
    }

    /// <summary>
    ///     Creates a new <see cref="SpriteBatch"/> snapshot from the current
    ///     settings of the given <see cref="SpriteBatch"/>.
    /// </summary>
    /// <param name="spriteBatch">
    ///     The <see cref="SpriteBatch" /> to take a snapshot of.
    /// </param>
    public SpriteBatchSnapshot(SpriteBatch spriteBatch)
    {
        SortMode = FnaAccessors.GetSortMode(spriteBatch);
        BlendState = FnaAccessors.GetBlendState(spriteBatch);
        SamplerState = FnaAccessors.GetSamplerState(spriteBatch);
        DepthStencilState = FnaAccessors.GetDepthStencilState(spriteBatch);
        RasterizerState = FnaAccessors.GetRasterizerState(spriteBatch);
        CustomEffect = FnaAccessors.GetCustomEffect(spriteBatch);
        TransformMatrix = FnaAccessors.GetTransformMatrix(spriteBatch);
    }

    /// <summary>
    ///     Initializes a set of parameters from this snapshot.
    /// </summary>
    public readonly SpriteBatchParameters ToParameters()
    {
        return new SpriteBatchParameters(
            SortMode,
            BlendState,
            SamplerState,
            DepthStencilState,
            RasterizerState,
            CustomEffect,
            TransformMatrix
        );
    }
}

/// <summary>
///     The small set of FNA implementation details needed by the rendering
///     helpers. tModLoader's source compiler does not run Publicizer, so these
///     accesses must be declared explicitly for the runtime implementation.
/// </summary>
internal static class FnaAccessors
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "beginCalled")]
    private static extern ref bool BeginCalledField(SpriteBatch spriteBatch);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "sortMode")]
    private static extern ref SpriteSortMode SortModeField(SpriteBatch spriteBatch);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "blendState")]
    private static extern ref BlendState BlendStateField(SpriteBatch spriteBatch);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "samplerState")]
    private static extern ref SamplerState SamplerStateField(SpriteBatch spriteBatch);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "depthStencilState")]
    private static extern ref DepthStencilState DepthStencilStateField(SpriteBatch spriteBatch);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "rasterizerState")]
    private static extern ref RasterizerState RasterizerStateField(SpriteBatch spriteBatch);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "customEffect")]
    private static extern ref Effect? CustomEffectField(SpriteBatch spriteBatch);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "transformMatrix")]
    private static extern ref Matrix TransformMatrixField(SpriteBatch spriteBatch);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_RenderTargetUsage")]
    private static extern void SetRenderTargetUsageMethod(RenderTarget2D target, RenderTargetUsage usage);

    internal static bool IsBegun(SpriteBatch spriteBatch) => BeginCalledField(spriteBatch);
    internal static SpriteSortMode GetSortMode(SpriteBatch spriteBatch) => SortModeField(spriteBatch);
    internal static BlendState GetBlendState(SpriteBatch spriteBatch) => BlendStateField(spriteBatch);
    internal static SamplerState GetSamplerState(SpriteBatch spriteBatch) => SamplerStateField(spriteBatch);
    internal static DepthStencilState GetDepthStencilState(SpriteBatch spriteBatch) => DepthStencilStateField(spriteBatch);
    internal static RasterizerState GetRasterizerState(SpriteBatch spriteBatch) => RasterizerStateField(spriteBatch);
    internal static Effect? GetCustomEffect(SpriteBatch spriteBatch) => CustomEffectField(spriteBatch);
    internal static Matrix GetTransformMatrix(SpriteBatch spriteBatch) => TransformMatrixField(spriteBatch);
    internal static void SetRenderTargetUsage(RenderTarget2D target, RenderTargetUsage usage) => SetRenderTargetUsageMethod(target, usage);
}

/// <summary>
///     Extensions to <see cref="SpriteBatch" /> using
///     <see cref="SpriteBatchSnapshot" /> instances.
/// </summary>
internal static class SpriteBatchSnapshotExtensions
{
    /// <param name="sb">The <see cref="SpriteBatch" />.</param>
    extension(SpriteBatch sb)
    {
        /// <summary>
        ///     Takes a snapshot of the <see cref="SpriteBatch" /> and then ends the
        ///     <see cref="SpriteBatch" />/
        /// </summary>
        /// <param name="ss">The produced <see cref="SpriteBatchSnapshot" />.</param>
        public void End(out SpriteBatchSnapshot ss)
        {
            ss = new SpriteBatchSnapshot(sb);
            sb.End();
        }

        /// <summary>
        ///     Starts a <see cref="SpriteBatch" /> with the parameters from the
        ///     given <see cref="SpriteBatchSnapshot" />.
        /// </summary>
        /// <param name="ss">The <see cref="SpriteBatchSnapshot" /> to use.</param>
        public void Begin(in SpriteBatchSnapshot ss)
        {
            sb.Begin(
                ss.SortMode,
                ss.BlendState,
                ss.SamplerState,
                ss.DepthStencilState,
                ss.RasterizerState,
                ss.CustomEffect,
                ss.TransformMatrix
            );
        }

        /// <summary>
        ///     Immediately ends and then starts the given <see cref="SpriteBatch" />
        ///     with the parameters from the given
        ///     <see cref="SpriteBatchSnapshot" />.
        /// </summary>
        /// <param name="ss">The <see cref="SpriteBatchSnapshot" /> to use.</param>
        public void Restart(in SpriteBatchSnapshot ss)
        {
            sb.End();
            sb.Begin(ss);
        }
    }
}
