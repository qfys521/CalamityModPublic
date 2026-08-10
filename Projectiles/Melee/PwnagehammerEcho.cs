using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class PwnagehammerEcho : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public static readonly SoundStyle BigSound = new("CalamityMod/Sounds/Item/PwnagehammerBigImpact") { Volume = 0.6f };
        public static readonly SoundStyle Kunk = new("CalamityMod/Sounds/Item/TF2PanHit") { Volume = 1.1f };
        public int Explodamage = 0;
        public float speed = 0f;
        public NPC targeted;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 7;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 248, 124, 255) with { A = 0 };
        }

        public override void AI()
        {
            speed = Projectile.velocity.Length();
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] < 42f)
            {
                Projectile.velocity.Y *= 0.9575f;
                Projectile.velocity.X *= 0.98f;
                Projectile.rotation += MathHelper.ToRadians(Projectile.ai[0] * 0.5f) * Projectile.localAI[0] * Projectile.direction;
            }
            else if (Projectile.ai[0] >= 42f)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 * 0.5f;
                Projectile.extraUpdates = 2;

                if (Projectile.ai[1] != -5)
                    targeted = Main.npc[(int)Projectile.ai[1]];
                if (targeted == null || !targeted.CanBeChasedBy(Projectile, false) || !targeted.active)
                {
                    Projectile.ai[1] = -5;
                    targeted = Projectile.Center.ClosestNPCAt(2000);
                }
                if (targeted != null)
                {
                    CalamityUtils.HomeInOnSelectedNPC(Projectile, targeted, true, 0.6f, 25, 0.98f);
                }
                else
                    Projectile.Kill();
            }

            if (Main.rand.NextBool())
            {
                Vector2 offset = new Vector2(7, 0).RotatedByRandom(MathHelper.ToRadians(360f));
                Vector2 velOffset = new Vector2(3, 0).RotatedBy(offset.ToRotation());
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.GoldFlame, new Vector2(Projectile.velocity.X * 0.2f + velOffset.X, Projectile.velocity.Y * 0.2f + velOffset.Y), 100, new Color(255, 245, 198), 2f);
                dust.noGravity = true;
            }

            if (Main.rand.NextBool(6))
            {
                Vector2 offset = new Vector2(7, 0).RotatedByRandom(MathHelper.ToRadians(360f));
                Vector2 velOffset = new Vector2(3, 0).RotatedBy(offset.ToRotation());
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.GoldFlame, new Vector2(Projectile.velocity.X * 0.2f + velOffset.X, Projectile.velocity.Y * 0.2f + velOffset.Y), 100, new Color(255, 245, 198), 2f);
                dust.noGravity = true;
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (targeted != null && target != targeted)
                modifiers.SourceDamage *= 0.3f;
        }
        public override bool? CanHitNPC(NPC target)
        {
            if (Projectile.ai[0] <= 42f)
                return false;
            return null;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target == targeted)
                Projectile.Kill();
        }

        public override bool PreKill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];

            float numberOfDusts = 45f;
            float rotFactor = 360f / numberOfDusts;
            for (int i = 0; i < numberOfDusts; i++)
            {
                float rot = MathHelper.ToRadians(i * rotFactor);
                Vector2 offset = new Vector2(15f, 0).RotatedBy(rot);
                Vector2 velOffset = new Vector2(12.5f, 0).RotatedBy(rot);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Sandnado, velOffset);
                dust.noGravity = true;
                dust.velocity = velOffset * (i % 2 == 0 ? 0.9f : i % 3 == 0 ? 0.8f : 1f);
                dust.scale = 3f;
            }

            if (Main.zenithWorld)
                SoundEngine.PlaySound(Kunk, Projectile.Center);

            else
                SoundEngine.PlaySound(BigSound, Projectile.Center);

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * 0f, ModContent.ProjectileType<PwnagehammerExplosionBig>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner, 0f);

            return false;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], Color.Gold with { A = 0 } * 0.5f, 1, texture, true, true);

            return false;
        }
    }
}
