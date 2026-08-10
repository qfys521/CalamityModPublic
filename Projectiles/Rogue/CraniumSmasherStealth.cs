using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class CraniumSmasherStealth : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/Rogue/CraniumSmasherExplosive";

        private NPC StickTarget;
        private Vector2 StickOffset;

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 300;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
            Projectile.tileCollide = false;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void AI()
        {
            // Sticking AI, ignore everything else if sticking
            if (StickTarget != null)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.Center = StickTarget.Center + StickOffset;
                Projectile.tileCollide = false;

                if (!StickTarget.active)
                    Projectile.Kill();
                return;
            }

            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] >= 5f)
                Projectile.tileCollide = true;

            Projectile.rotation += Projectile.velocity.X * 0.02f;
            Projectile.velocity.Y += 0.085f;
            Projectile.velocity.X *= 0.99f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<HeavyBleeding>(), 300);
            if (StickTarget == null)
            {
                StickTarget = target;
                StickOffset = Projectile.Center - target.Center;
                if (Projectile.timeLeft < 62)
                    Projectile.timeLeft = 62;
            }

            if (Projectile.penetrate > 1)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<CraniumSMASH>(), Projectile.damage, 0f, Projectile.owner);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<HeavyBleeding>(), 300);

        public override void OnKill(int timeLeft)
        {
            if (Main.myPlayer == Projectile.owner)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<CraniumSMASH>(), (int)(Projectile.damage * 1.5f), 0f, Projectile.owner, 0f, 1f);
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            Main.LocalPlayer.SetScreenshake(3.5f);

            if (!Main.dedServ)
            {
                int goreAmt = 3;
                Vector2 source = Projectile.Center - new Vector2(24f);
                for (int goreIndex = 1; goreIndex <= goreAmt; goreIndex++)
                {
                    float velocityMult = 0.33f * goreIndex;
                    int type = Main.rand.Next(61, 63 + 1);
                    Gore smoke = Gore.NewGoreDirect(Projectile.GetSource_Death(), source, default, type, 1f);
                    smoke.velocity *= velocityMult;
                    type = Main.rand.Next(61, 63 + 1);
                    smoke = Gore.NewGoreDirect(Projectile.GetSource_Death(), source, default, type, 1f);
                    smoke.velocity *= velocityMult;
                }
            }

            for (int i = 0; i < 30; i++)
            {
                float edgeOffset = Main.rand.NextFloat(60f, 100f) * (Main.rand.NextBool() ? -1 : 1);
                float randOffset = Main.rand.NextFloat(-100f, 100f);
                Vector2 spawnPos = Projectile.Center + (i % 2 == 0 ? new Vector2(edgeOffset, randOffset) : new Vector2(randOffset, edgeOffset));
                Dust dust = Dust.NewDustPerfect(spawnPos, DustID.IceTorch, Vector2.Zero, 100, default, 2f);
                dust.noGravity = true;
            }
        }

        public override void PostDraw(Player player, Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Rogue/CraniumSmasherGlow").Value;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
        }
    }
}
