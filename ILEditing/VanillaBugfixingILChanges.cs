using CalamityMod.Systems.Collections;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.ILEditing
{
    public partial class ILChanges
    {
        #region Fixing Vanilla Not Accounting For Spritebatch Modification in Held Projectiles
        private static bool HasLoggedHeldProjectileBlendStateCatch = false;
        private static void FixHeldProjectileBlendState(On_PlayerDrawLayers.orig_DrawHeldProj orig, PlayerDrawSet drawinfo, Projectile proj)
        {
            orig(drawinfo, proj);

            // Vanilla uses a worse quality sampler state for mounts when moving for some reason. Really couldn't say why.
            var sampler = (drawinfo.drawPlayer.mount.Active && drawinfo.drawPlayer.fullRotation != 0f) ? LegacyPlayerRenderer.MountedSamplerState : Main.DefaultSamplerState;

            try
            {
                // Restart the spritebatch, to ensure that modifications made to it are properly restored.
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, sampler, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch
            {
                if (!HasLoggedHeldProjectileBlendStateCatch)
                    LogFailure("FixHeldProjectileBlendState", "The spritebatch was not left properly by another mod! The game will now most likely crash.");

                HasLoggedHeldProjectileBlendStateCatch = true;
            }
        }
        #endregion

        #region Fix Vanilla Not Accounting For Multiple Bobbers When Fishing With Truffle Worm
        [System.ThreadStatic]
        private static bool truffleWormUsedForCurrentPull;

        private static bool FixTruffleWormFishing(On_Player.orig_ItemCheck_PullFishingBobbers orig, Player self, Item sItem)
        {
            bool previousState = truffleWormUsedForCurrentPull;
            truffleWormUsedForCurrentPull = false;
            try
            {
                return orig(self, sItem);
            }
            finally
            {
                truffleWormUsedForCurrentPull = previousState;
            }
        }

        private static bool PreventRepeatedTruffleWormUse(On_Player.orig_ItemCheck_CheckFishingBobber_ConsumeBait orig, Player self, Projectile bobber, out int baitTypeUsed)
        {
            if (truffleWormUsedForCurrentPull)
            {
                baitTypeUsed = 0;
                return false;
            }

            bool result = orig(self, bobber, out baitTypeUsed);
            if (result && baitTypeUsed == ItemID.TruffleWorm)
                truffleWormUsedForCurrentPull = true;

            return result;
        }
        #endregion

        #region Fix Vanilla does not call CheckDead when NPC has realLife
        private static void EnsureCheckDeadOnSegments(ILContext il)
        {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdfld<NPC>(nameof(NPC.realLife)),
                i => i.MatchLdelemRef(),
                i => i.MatchCallOrCallvirt<NPC>(nameof(NPC.checkDead))
                ))
            {
                LogFailure("EnsureCheckDeadOnSegments", "Could not locate the checkDead instruction sets");
                return;
            }

            cursor.EmitLdarg0();
            cursor.EmitDelegate((NPC npc) =>
            {
                if (npc.life <= 0 && CalamityNPCSets.DoCheckDeadRegardlessRealLife[npc.type])
                {
                    NPCLoader.CheckDead(npc);
                }
            });
        }
        #endregion
    }
}
