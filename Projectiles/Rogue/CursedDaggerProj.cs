using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class CursedDaggerProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/CursedDagger";
        public bool slowstart = true;
        public int speedup = 0;
        public override void SetDefaults()
        {
            Projectile.localNPCHitCooldown = 10;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 600;

            Projectile.width = Projectile.height = 38;
            Projectile.aiStyle = ProjAIStyleID.ThrownProjectile;
            AIType = ProjectileID.ThrowingKnife;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            if (Collision.SolidCollision(Projectile.Center, 3, 3))
                Projectile.Kill();

            if (Main.rand.NextBool(Projectile.Calamity().stealthStrike ? 3 : 7))
            {
                Dust dust = Main.dust[Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, (int)CalamityDusts.SulphurousSeaAcid, Projectile.velocity.X * -3f, Projectile.velocity.Y * -3f, 0, default, Projectile.Calamity().stealthStrike ? Main.rand.NextFloat(2.5f, 3.2f) : 1.5f)];
                dust.noGravity = true;
            }

            if (Projectile.Calamity().stealthStrike)
            {
                if (slowstart)
                {
                    Projectile.velocity *= 0.5f;
                    Projectile.penetrate = 1;
                    slowstart = false;
                }
                if (speedup <= 50)
                {
                    Projectile.velocity *= 1.03f;
                    speedup++;
                }
                Projectile.aiStyle = 0;
                Projectile.extraUpdates = 1;
            }
        }

        public override void OnKill(int timeLeft)
        {
            int numberofdusts = Projectile.Calamity().stealthStrike ? 14 : 5;
            for (int i = 0; i <= numberofdusts; i++)
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, (int)CalamityDusts.SulphurousSeaAcid, Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f);
            }
            if (Projectile.Calamity().stealthStrike)
            {
                SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact, Projectile.position);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<CursedDaggerBlastHitbox>(), (int)(Projectile.damage), Projectile.knockBack * 2, Projectile.owner, 0, 0f);
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.CursedInferno, Projectile.Calamity().stealthStrike ? 900 : 90);

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.8f);
            if (Projectile.damage < 1)
                Projectile.damage = 1;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(BuffID.CursedInferno, Projectile.Calamity().stealthStrike ? 600 : 120);
    }
}
