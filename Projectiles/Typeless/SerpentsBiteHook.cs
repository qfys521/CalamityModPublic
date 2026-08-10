using CalamityMod.Items.Fishing.SunkenSeaCatches;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Typeless
{
    public class SerpentsBiteHook : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";

        public static Asset<Texture2D> Chain;
        public override void Load() => Chain = Request<Texture2D>("CalamityMod/Projectiles/Typeless/SerpentsBiteChain");

        public override void SetDefaults() => Projectile.CloneDefaults(ProjectileID.GemHookAmethyst);

        public override float GrappleRange() => SerpentsBite.Reach * 16f;
        public override void GrappleRetreatSpeed(Player player, ref float speed) => speed = SerpentsBite.ReelbackSpeed;
        public override void GrapplePullSpeed(Player player, ref float speed) => speed = SerpentsBite.PullSpeed;

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
            if (hooksOut > 2) // This hook can have 3 hooks out.
            {
                return false;
            }
            return true;
        }
        public override void NumGrappleHooks(Player player, ref int numHooks) => numHooks = 2;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ => Projectile.DrawHook(Chain.Value);

        public override void AI()
        {
            Projectile.spriteDirection = -Projectile.direction;
        }
    }
}
