using System.IO;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    [PierceResistException]
    public class SacrificeProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public Player Owner => Main.player[Projectile.owner];
        public bool StickingToAnything => Projectile.ai[0] == 1f;
        public bool ReturningToOwner => Projectile.ai[0] == 2f;
        public bool AbleToHealOwner = true;
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/Sacrifice";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 1;
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 62;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(AbleToHealOwner);

        public override void ReceiveExtraAI(BinaryReader reader) => AbleToHealOwner = reader.ReadBoolean();


        public override void AI()
        {
            if (Projectile.Calamity().LocketClone)
                AbleToHealOwner = false; // No healing you too much of your hp

            if (ReturningToOwner)
            {
                Projectile.timeLeft = 20;
                Projectile.velocity = Projectile.SafeDirectionTo(Owner.Center) * 28f;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi + MathHelper.PiOver4;
                Projectile.damage = 0;

                // Heal the player and disappear when touching them.
                if (Projectile.Hitbox.Intersects(Owner.Hitbox))
                {
                    if (AbleToHealOwner)
                        Owner.DoLifestealDirect(null, Projectile.Calamity().stealthStrike ? 40 : 3, 0.4f);
                    Projectile.Kill();
                }
            }
            else if (!StickingToAnything)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            else if (!Main.dedServ && Projectile.timeLeft % 40f == 39f)
            {
                for (int i = 0; i < 60; i++)
                {
                    Dust blood = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(50f, 50f), DustID.RainbowMk2);
                    blood.velocity = Main.rand.NextVector2Circular(3f, 3f);
                    blood.noGravity = true;
                    blood.color = Color.Lerp(Color.Red, Color.DarkRed, Main.rand.NextFloat(0.25f, 1f));
                    blood.scale = Main.rand.NextFloat(1f, 1.4f);
                }
            }

            if (StickingToAnything)
            {
                if (Projectile.timeLeft > 90 && !Projectile.Calamity().stealthStrike)
                    Projectile.timeLeft = 90;
                else if (Projectile.timeLeft > 180 && Projectile.Calamity().stealthStrike)
                    Projectile.timeLeft = 180;
            }
            Projectile.StickyProjAI(50);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Projectile.ModifyHitNPCSticky(15);
            Projectile.velocity *= 0.5f;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust blood = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.Blood, 0, 0, 0, default, 1.5f);
                blood.position += Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.scale * 42f;
                blood.noGravity = true;
            }
        }
    }
}
