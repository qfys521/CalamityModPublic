#nullable enable

using System;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityMod.Utilities.Daybreak;

/// <summary>
///     Starts a <see cref="SpriteBatch"/> and caches its old start parameters
///     if it was begun to restore it on disposal.
/// </summary>
internal readonly struct SpriteBatchScope : IDisposable
{
    private readonly SpriteBatch spriteBatch;
    private readonly SpriteBatchSnapshot? oldState;

    /// <summary>
    ///     Initializes a new scope. If the <see cref="SpriteBatch"/> has
    ///     already begun, its current parameters are saved and the
    ///     <see cref="SpriteBatch"/> is ended; the original parameters will
    ///     then be reapplied on disposal.
    /// </summary>
    /// <param name="spriteBatch">The <see cref="SpriteBatch"/>.</param>
    public SpriteBatchScope(SpriteBatch spriteBatch)
    {
        this.spriteBatch = spriteBatch;

        if (!FnaAccessors.IsBegun(spriteBatch))
        {
            return;
        }

        spriteBatch.End(out var old);
        oldState = old;
    }

    /// <summary>
    ///     Ends the <see cref="SpriteBatch"/> and starts it with the old
    ///     parameters if it has already begun prior.
    /// </summary>
    public void Dispose()
    {
        if (oldState.HasValue)
        {
            spriteBatch.Begin(oldState.Value);
        }
    }
}

/// <summary>
///     Extensions to types for <see cref="SpriteBatch"/> scopes.
/// </summary>
internal static class SpriteBatchScopeExtensions
{
    /// <summary>
    ///     See <see cref="SpriteBatchScope"/>.
    /// </summary>
    public static SpriteBatchScope Scope(this SpriteBatch @this)
    {
        return new SpriteBatchScope(@this);
    }
}
