using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class PhotosyntheticSolarBeam : BaseLaserbeamProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override float MaxScale => 1f;
        public override float MaxLaserLength => 1200f;
        public override float Lifetime => 30f;
        public override Color LightCastColor => Color.White;
        public override Texture2D LaserBeginTexture => ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Lasers/UltimaRayStart", AssetRequestMode.ImmediateLoad).Value;
        public override Texture2D LaserMiddleTexture => ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Lasers/UltimaRayMid", AssetRequestMode.ImmediateLoad).Value;
        public override Texture2D LaserEndTexture => ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Lasers/UltimaRayEnd", AssetRequestMode.ImmediateLoad).Value;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = (int)Lifetime;
        }

        public override void ExtraBehavior()
        {
            // Generate a star-like and circular burst of terra dust.
            if (!Main.dedServ && Time == 5f)
            {
                int starPoints = 6;
                for (int i = 0; i < starPoints; i++)
                {
                    float angle = MathHelper.TwoPi * i / starPoints;
                    for (int j = 0; j < 6; j++)
                    {
                        float starSpeed = MathHelper.Lerp(1f, 7f, j / 6f);
                        Color dustColor = Color.Lerp(Color.White, Color.YellowGreen, j / 6f);
                        float dustScale = MathHelper.Lerp(1.6f, 0.85f, j / 6f);

                        Dust terraMagic = Dust.NewDustPerfect(Projectile.Center, DustID.Terra);
                        terraMagic.velocity = angle.ToRotationVector2() * starSpeed;
                        terraMagic.color = dustColor;
                        terraMagic.scale = dustScale;
                        terraMagic.noGravity = true;
                    }
                }

                int ovalPoints = 30;
                for (int i = 0; i < ovalPoints; i++)
                {
                    float angle = MathHelper.TwoPi * i / ovalPoints;
                    Dust terraMagic = Dust.NewDustPerfect(Projectile.Center, DustID.Terra);
                    terraMagic.velocity = angle.ToRotationVector2() * 6f;
                    terraMagic.scale = 1.1f;
                    terraMagic.noGravity = true;
                }
            }
        }

        public override void DetermineScale() => Projectile.scale = Projectile.timeLeft / Lifetime * MaxScale;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            DrawBeamWithColor(Color.Lime * 1.1f, Projectile.scale);
            DrawBeamWithColor(Color.Yellow * 1.1f, Projectile.scale * 0.5f);
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.95f);
            if (Projectile.damage < 1)
                Projectile.damage = 1;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 2; i++)
            {
                int tries = 0;
                Vector2 spawnOffset;
                do
                {
                    spawnOffset = Main.rand.NextVector2CircularEdge(target.width * 0.5f + 40f, target.height * 0.5f + 40f);
                    tries++;
                }
                while (Collision.SolidCollision((target.Center + spawnOffset).ToTileCoordinates().ToVector2(), 4, 4) && tries < 10);

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center + spawnOffset, Main.rand.NextVector2CircularEdge(6f, 6f), ModContent.ProjectileType<PhotosyntheticShard>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner);
            }
        }
    }
}
