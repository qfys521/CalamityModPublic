using System;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Systems.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class WalkingCaneProjectile : BaseSpearProjectile
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<WalkingCane>();
        public static Asset<Texture2D> AltTexture;

        private bool initialized = false;

        public override void SetStaticDefaults()
        {
            if (!Main.dedServ)
                AltTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Rogue/WalkingCaneProjectileAlt");
        }
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.timeLeft = 120;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.ownerHitCheck = true;
            Projectile.hide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.alpha = 180;
            Projectile.scale = 1.25f;
        }
        public override SpearType SpearAiType => SpearType.GhastlyGlaiveSpear;
        public override float TravelSpeed => 35f;

        public override bool PreAI()
        {
            // Initialization. Using the AI hook would override the base spear's code, and we don't want that.
            if (!initialized)
            {
                Main.player[Projectile.owner].Calamity().ConsumeStealthByAttacking();
                initialized = true;
            }
            return true;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Vector2 drawPosition = Projectile.position + new Vector2(Projectile.width, Projectile.height) / 2f + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
            Texture2D alternateHookTexture = Projectile.spriteDirection == -1 ? AltTexture.Value : Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 origin = new Vector2(Projectile.spriteDirection == 1 ? alternateHookTexture.Width + 8f : -8f, -8f);
            
            Main.EntitySpriteDraw(alternateHookTexture, drawPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float spearLengthMult = 3.2f;
            float velocityMagnitude = 0f;
            // For whatever fucking reason CheckAABBvLineCollision fails when you're completely inside a hitbox, so check for that too
            return targetHitbox.Contains(Main.LocalPlayer.Center.ToPoint()) || Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                Main.player[Projectile.owner].Center, Main.player[Projectile.owner].Center + Projectile.velocity * spearLengthMult, (TravelSpeed + 1f) * Projectile.scale, ref velocityMagnitude);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.Calamity().stealthStrike)
            {
                target.Calamity().caneInsanityTimer = 600;
            }
            else if (target.Calamity().caneInsanityTimer > 0)
            {
                Vector2 position = target.Center + Main.rand.NextVector2CircularEdge(100f, 100f);
                Vector2 velocity = (target.Center - position).SafeNormalize(Vector2.Zero) * 6f;
                // Friendly shadow hands normally have four different behaviors, but Walking Cane only spawns shadow hands that immediately lunge
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, velocity, ProjectileID.InsanityShadowFriendly, (int)(Projectile.damage * 0.75f), Projectile.knockBack, Projectile.owner);
            }
        }
    }
}
