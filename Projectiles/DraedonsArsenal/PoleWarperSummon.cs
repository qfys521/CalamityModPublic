using System;
using CalamityMod.Buffs.Summon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.DraedonsArsenal
{
    public class PoleWarperSummon : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public float AngularOffset = 0f;
        public const float MaximumRepulsionSpeed = 11f;
        public const float ChargeTime = 45f;
        public float Time
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public bool North
        {
            get => Projectile.ai[1] == 1f;
            set => Projectile.ai[1] = value.ToInt();
        }
        public override void SetStaticDefaults()
        {
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargetingFeature[Type] = true;
            ProjectileID.Sets.TrailingMode[Type] = 0;
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 22;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.minionSlots = 0.5f;
            Projectile.timeLeft = 18000;
            Projectile.penetrate = -1;
            Projectile.timeLeft *= 5;
            Projectile.minion = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (Projectile.localAI[0] == 0f)
            {
                Initialize(player);
                Projectile.localAI[0] = 1f;
            }
            GrantBuffs(player);
            NPC potentialTarget = Projectile.Center.MinionHoming(2000f, player);

            // Teleport near the target if very far away from them.
            if (!Projectile.WithinRange(player.Center, 4200f))
            {
                Projectile.Center = player.Center + Vector2.UnitY * North.ToDirectionInt() * 25f;
            }

            if (potentialTarget is null)
            {
                PlayerMovement(player);
                RepelMovement();
            }
            else
            {
                NPCMovement(potentialTarget);
                if (Time % ChargeTime < 35f)
                {
                    RepelMovement();
                }
            }
            Time++;
        }

        public void Initialize(Player player)
        {
            for (int i = 0; i < 45; i++)
            {
                float angle = MathHelper.TwoPi / 45f * i;
                Vector2 velocity = angle.ToRotationVector2() * 4f;
                Dust dust = Dust.NewDustPerfect(Projectile.Center + velocity * 2.75f, DustID.AncientLight, velocity);
                dust.noGravity = true;
            }
        }

        public void GrantBuffs(Player player)
        {
            bool isCorrectProjectile = Projectile.type == ModContent.ProjectileType<PoleWarperSummon>();
            player.AddBuff(ModContent.BuffType<PoleWarperBuff>(), 3600);
            if (isCorrectProjectile)
            {
                if (player.dead)
                {
                    player.Calamity().poleWarper = false;
                }
                if (player.Calamity().poleWarper)
                {
                    Projectile.timeLeft = 2;
                }
            }
        }

        public void PlayerMovement(Player player)
        {
            Vector2 destination = player.Center + Vector2.UnitY.RotatedBy(Time / 16f + AngularOffset + (!North).ToInt() * MathHelper.Pi) * 180f;
            Projectile.velocity = (Projectile.velocity * 4f + Projectile.SafeDirectionTo(destination) * 10f) / 5f;
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 10f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public void NPCMovement(NPC npc)
        {
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.type == Projectile.type &&
                    Projectile.owner == Projectile.owner)
                {
                    PoleWarperSummon otherPole = (PoleWarperSummon)p.ModProjectile;
                    if (otherPole.Time != Time && otherPole.Time != Time + 1)
                    {
                        otherPole.Time = Time;
                    }
                }
            }
            if (Time % ChargeTime < 20f)
            {
                float offsetAngle = AngularOffset * 0.5f + (!North).ToInt() * MathHelper.Pi;
                Vector2 destination = npc.Center + Vector2.UnitY.RotatedBy(offsetAngle) * 180f;
                Projectile.velocity = (Projectile.velocity * 4f + Projectile.SafeDirectionTo(destination) * 10f) / 5f;
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 30f;
                Projectile.rotation = Projectile.AngleTo(npc.Center) + MathHelper.PiOver2;
            }
            else if (Time % ChargeTime < 35f)
            {
                Projectile.velocity *= 0.96f;
                Projectile.rotation += 0.05f;
            }
            else if (Time % ChargeTime == 35f)
            {
                Projectile.velocity = Projectile.SafeDirectionTo(npc.Center, -Vector2.UnitY) * 20f;
                Projectile.rotation = Projectile.AngleTo(npc.Center) + MathHelper.PiOver2;
            }
        }

        public void RepelMovement()
        {
            // This does not incorporate attraction on purpose. Doing so causes the minions to very easily become distracted.
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.type == Projectile.type && Projectile.Distance(p.Center) < 40f)
                {
                    PoleWarperSummon otherPole = (PoleWarperSummon)p.ModProjectile;
                    if (otherPole.North != North)
                    {
                        float distanceFromOtherPole = Projectile.Distance(p.Center) + 1f;
                        if (float.IsNaN(distanceFromOtherPole) || distanceFromOtherPole < 1f)
                        {
                            distanceFromOtherPole = 1f;
                        }
                        float repulsionSpeed = MaximumRepulsionSpeed * (float)Math.Pow(3f, -distanceFromOtherPole / 27f);
                        Projectile.velocity -= (p.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * repulsionSpeed;
                    }
                }
            }
        }

        public override bool MinionContactDamage() => true;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Color drawColor = Projectile.GetAlpha(lightColor);

            if (CalamityClientConfig.Instance.Afterimages)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (i % 2 == 0)
                        continue;

                    Color trailColor = Color.Lerp(drawColor, Color.Transparent, i / (float)Projectile.oldPos.Length) * 0.67f;
                    Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                    Main.EntitySpriteDraw(tex, trailPos, null, trailColor, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
                }
            }

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
