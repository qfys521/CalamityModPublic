using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class EnchantedAxeProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/EnchantedAxe";

        private bool recall = false;
        private bool summonAxe = true;
        private int Lifetime = 600;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void AI()
        {
            if (Projectile.Calamity().stealthStrike)
            {
                if (Projectile.timeLeft < Lifetime - 18)
                {
                    recall = true;
                    Projectile.tileCollide = false;
                }
            }
            else
            {
                if (Projectile.timeLeft < Lifetime - 10)
                {
                    recall = true;
                    Projectile.tileCollide = false;
                }
            }

            Projectile.rotation += 0.4f * Projectile.direction;

            if (recall)
            {
                Vector2 posDiff = Main.player[Projectile.owner].position - Projectile.position;
                if (posDiff.Length() > 30f)
                {
                    posDiff.Normalize();
                    Projectile.velocity = posDiff * 30f;
                }
                else
                {
                    Projectile.timeLeft = 0;
                    OnKill(Projectile.timeLeft);
                }

                if (summonAxe)
                {
                    SummonAxe(true);
                }
            }
            else
            {
                if (Projectile.timeLeft % 3 == 1 && Projectile.Calamity().stealthStrike)
                {
                    SummonAxe(false);
                }
            }

            if (Projectile.position == Main.player[Projectile.owner].position)
            {
                OnKill(Projectile.timeLeft);
            }
            return;
        }

        public void SummonAxe(bool recall)
        {
            float minDist = 999f;
            int index = 0;
            // Get the closest enemy to the axe
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.CanBeChasedBy(Projectile, false))
                {
                    float dist = (Projectile.Center - npc.Center).Length();
                    if (dist < minDist)
                    {
                        minDist = dist;
                        index = npc.whoAmI;
                    }
                }
            }
            Vector2 newAxeVelocity;
            if (minDist < 999f)
                newAxeVelocity = Main.npc[index].Center - Projectile.Center;
            else
                newAxeVelocity = -Projectile.velocity;

            newAxeVelocity.Normalize();
            newAxeVelocity *= 20f;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position, newAxeVelocity, ModContent.ProjectileType<EnchantedAxe2>(), Projectile.damage, 2, Projectile.owner, recall ? 0f : 1f);
            if (recall)
                summonAxe = false;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (recall)
            {
                return false;
            }
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            recall = true;
            Projectile.tileCollide = false;
            return false;
        }
    }
}
