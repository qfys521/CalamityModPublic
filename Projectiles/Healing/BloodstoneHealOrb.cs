using CalamityMod.Graphics.Metaballs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Healing
{
    public class BloodstoneHealOrb : ModProjectile, ILocalizedModType //Modified copy from SanguineHealOrb
    {
        public new string LocalizationCategory => "Projectiles.Healing";

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public int target = -1;
        public ref int heal => ref Projectile.damage;

        public int spawnCooldown = 60; //how many frames must pass before it can be picked up

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 3;
            Projectile.timeLeft = 60 * 5 * Projectile.MaxUpdates;
            spawnCooldown *= Projectile.MaxUpdates;
        }
        public override void AI()
        {
            bool finalUpdate = Projectile.FinalExtraUpdate();

            var p = BloodMetaball.SpawnParticle(Projectile.Center + Projectile.velocity, Main.rand.NextVector2Circular(-0.5f, -0.5f), Projectile.width);
            p.SizeScaling = 0.75f;
            p.ShrinkDelay = 1;

            float maxDistanceSq = 200f * 200f;
            if (spawnCooldown > 0)
            {
                PassiveBehavior();
                spawnCooldown--;
                return;
            }
            if (target < 0)
            {
                PassiveBehavior();
                if (finalUpdate) for (int playerIndex = 0; playerIndex < Main.maxPlayers; playerIndex++)
                    {
                        Player player = Main.player[playerIndex];
                        float perPlayerMaxDistanceSq = player.lifeMagnet ? maxDistanceSq * 2.25f : maxDistanceSq; //Heartreach gives 50% more range
                        float targetDistSq = Vector2.DistanceSquared(player.Center, Projectile.Center);
                        if (targetDistSq < perPlayerMaxDistanceSq)
                        {
                            maxDistanceSq = targetDistSq;
                            target = playerIndex;
                        }
                    }
            }
            else HealHome();
        }

        public void PassiveBehavior()
        {
            Projectile.velocity *= 0.99f;
        }

        public void HealHome()
        {
            Player player = Main.player[target];
            Vector2 playerVector = player.Center - Projectile.Center;
            float playerDist = playerVector.Length();
            if (playerDist < 50f && Projectile.position.X < player.position.X + player.width && Projectile.position.X + Projectile.width > player.position.X && Projectile.position.Y < player.position.Y + player.height && Projectile.position.Y + Projectile.height > player.position.Y)
            {
                Heal(player, heal);
                Projectile.Kill();
            }

            Projectile.velocity = (Projectile.velocity * 5 + (playerVector.SafeNormalize(Vector2.Zero) * 5)) / 6f; //Move towards player. Range is determined in the AI();
        }

        public static void Heal(Player player, int PotionTime)
        {
            var cplayer = player.Calamity();
            if (player.potionDelay > 0)
            {
                player.potionDelay -= PotionTime;
                if (player.potionDelay < 0)
                    player.potionDelay = 0;
                if (player.HasBuff(BuffID.PotionSickness))
                {
                    for (var i = 0; i < player.buffType.Length; i++)
                    {
                        if (player.buffType[i] == BuffID.PotionSickness)
                        {
                            player.buffTime[i] = player.potionDelay;
                        }
                    }
                }
            }
            else
            {
                player.lifeRegenTime += 3*PotionTime; //if Potion Sickness is full, each orb speeds up natural regen
            }

            Particle ring = new CustomPulse(player.Center, Vector2.Zero, (!ChildSafety.Disabled ? Color.CornflowerBlue : new Color(255, 32, 32)) * 0.75f, "CalamityMod/Particles/DustyCircleHardEdge", Vector2.One, 0, 0.01f, 0.05f, 20);
            GeneralParticleHandler.SpawnParticle(ring);
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            return false;
        }
    }
}
