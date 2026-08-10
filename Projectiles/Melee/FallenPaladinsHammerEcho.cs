using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class FallenPaladinsHammerEcho : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public static readonly SoundStyle SlamHamSound = new("CalamityMod/Sounds/Item/FallenPaladinsHammerBigImpact") { Volume = 0.6f };
        public static readonly SoundStyle Kunk = new("CalamityMod/Sounds/Item/TF2PanHit") { Volume = 1.1f };
        public float speed = 0f;
        public NPC targeted;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 15;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 62;
            Projectile.height = 62;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] < 42f)
            {
                Projectile.velocity *= 0.95f;
                Projectile.rotation += (1f * Utils.GetLerpValue(30, 0, Projectile.ai[0]) * Projectile.direction);
            }
            else if (Projectile.ai[0] >= 42f)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 * 0.5f;
                Projectile.extraUpdates = 4;

                if (Projectile.ai[1] != -5)
                    targeted = Main.npc[(int)Projectile.ai[1]];
                if (targeted == null || !targeted.CanBeChasedBy(Projectile, false) || !targeted.active)
                {
                    Projectile.ai[1] = -5;
                    targeted = Projectile.Center.ClosestNPCAt(2000);
                }
                if (targeted != null)
                {
                    CalamityUtils.HomeInOnSelectedNPC(Projectile, targeted, true, 0.7f, 25, 0.98f);
                }
                else
                    Projectile.Kill();
            }
            if (Projectile.ai[0] == 42f && targeted != null)
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.FireworksRGB, ((targeted.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 15).RotatedByRandom(0.4f) * Main.rand.NextFloat(0.2f, 1f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.5f, 1.2f);
                    dust.color = Main.rand.NextBool() ? Color.IndianRed : Color.Red;
                }
            }

            if (Main.rand.NextBool())
            {
                Vector2 offset = new Vector2(7, 0).RotatedByRandom(MathHelper.ToRadians(360f));
                Vector2 velOffset = new Vector2(3, 0).RotatedBy(offset.ToRotation());
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.RainbowMk2, new Vector2(Projectile.velocity.X * 0.2f + velOffset.X, Projectile.velocity.Y * 0.2f + velOffset.Y), 0, new Color(255, 245, 198), 1f);
                dust.noGravity = true;
                dust.color = Color.DarkRed;
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

        public override bool CanHitPvp(Player target) => Projectile.ai[0] > 42f;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 180);
            if (target == targeted)
                Projectile.Kill();
        }

        public override bool PreKill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            //This is what we call fucking IMPACT.
            if (Main.zenithWorld)
                SoundEngine.PlaySound(Kunk, Projectile.Center);

            else
                SoundEngine.PlaySound(SlamHamSound, Projectile.Center);
            Main.player[Projectile.owner].SetScreenshake(5f);

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * 0.001f, ModContent.ProjectileType<FallenBlast>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner, 0f);

            return false;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/FallenPaladinsHammer").Value;
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], Color.DarkRed with { A = 0 } * 0.5f, 1, texture, true, true);

            Projectile.DrawProjectileWithBackglow(Color.Red with { A = 0 }, Color.Lerp(Color.Red, Color.White, 0.5f), 5f, texture);
            return false;
        }
    }
}
