using CalamityMod.Items.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Typeless
{
    public class BobbitHead : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";

        public static Asset<Texture2D> Chain;
        public override void Load() => Chain = Request<Texture2D>("CalamityMod/Projectiles/Typeless/BobbitHookChain");

        public override void SetDefaults() => Projectile.CloneDefaults(ProjectileID.GemHookAmethyst);

        public override float GrappleRange() => BobbitHook.Reach * 16f;
        public override void GrappleRetreatSpeed(Player player, ref float speed) => speed = BobbitHook.ReelbackSpeed;
        public override void GrapplePullSpeed(Player player, ref float speed) => speed = BobbitHook.PullSpeed;

        // Use this hook for hooks that can have multiple hooks mid-flight: Dual Hook, Web Slinger, Fish Hook, Static Hook, Lunar Hook
        public override bool? CanUseGrapple(Player player)
        {
            int hooksOut = 0;
            for (int l = 0; l < Main.maxProjectiles; l++)
            {
                if (Main.projectile[l].active && Main.projectile[l].owner == Main.myPlayer && Main.projectile[l].type == Projectile.type)
                {
                    hooksOut++;
                }
            }
            if (hooksOut > 1) // This hook can have 2 hooks out.
            {
                return false;
            }
            return true;
        }
        public override void NumGrappleHooks(Player player, ref int numHooks) => numHooks = 1;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ => Projectile.DrawHook(Chain.Value);

        public override void AI()
        {
            Projectile.spriteDirection = -Projectile.direction;

            if (Projectile.ai[0] == 2f)
                Projectile.extraUpdates = 1;
            else
                Projectile.extraUpdates = 0;
        }
    }
}
