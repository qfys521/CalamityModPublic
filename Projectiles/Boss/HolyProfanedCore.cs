using CalamityMod.Items.SummonItems;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class HolyProfanedCore : ModProjectile, ILocalizedModType
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName(ModContent.ItemType<ProfanedCore>());
        public override string Texture => "CalamityMod/Items/SummonItems/ProfanedCore";

        public const int Lifetime = 180;
        public const int ShakeThreshold = 90;
        public ref float Timer => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = Lifetime;
        }

        public override void AI()
        {
            var Prov = CalamityGlobalNPC.holyBoss;
            // Abort if Provi does not exist
            if (Prov == -1)
            {
                Projectile.active = false;
                return;
            }

            Timer++;

            // First, briefly flies up
            // Then, moves towards the position of Provi's core on her sprite, speed scales based on distance to make it smooth
            if (Timer <= 30)
            {
                Projectile.velocity.Y = -4.5f;
            }
            else if (Timer > 30 && Timer <= Lifetime)
            {
                Projectile.velocity = Vector2.Zero;
                Vector2 proviCoreLocation = Main.npc[Prov].Center + new Vector2(0f, 40f);
                Projectile.Center += (proviCoreLocation - Projectile.Center) * 0.0375f;

                // Awesome particle effects or something
                if (Timer > ShakeThreshold)
                {
                    Color flameColor = new Color(255, 223, 112), crystalColor = new Color(190, 141, 184);

                    float starScale = MathHelper.Lerp(0f, 6f, (Timer - ShakeThreshold) / (Lifetime - ShakeThreshold));
                    CustomSpark attacka = new(proviCoreLocation, Vector2.Zero, "CalamityMod/Particles/FullStar", false, 2, starScale, flameColor, Vector2.One, extraRotation: MathHelper.PiOver4);
                    GeneralParticleHandler.SpawnParticle(attacka);
                    CustomSpark attacka2 = new(proviCoreLocation, Vector2.Zero, "CalamityMod/Particles/FullStar", false, 2, starScale * 0.4f, flameColor, Vector2.One);
                    GeneralParticleHandler.SpawnParticle(attacka2);

                    if (Timer % 2 == 0)
                    {
                        CustomSpark crystal = new(proviCoreLocation + Main.rand.NextVector2Circular(300, 300), -Vector2.UnitY * 2f, "CalamityMod/Particles/ProvidenceMarkParticle", false, 15, 2f, crystalColor, Vector2.One, fadeIn: true);
                        GeneralParticleHandler.SpawnParticle(crystal);
                    }
                    else
                    {
                        Vector2 spawnLocation = proviCoreLocation + Main.rand.NextVector2Circular(250, 250);
                        CustomSprite rockConverge = new(spawnLocation, (proviCoreLocation - spawnLocation) * 0.08f, 15, "CalamityMod/NPCs/ProfanedGuardians/ProfanedRocks" + Main.rand.Next(1, 6 + 1), 0.3f, Color.White, 0f, false);
                        rockConverge.Rotation = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                        GeneralParticleHandler.SpawnParticle(rockConverge);
                    }
                }
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            float shakeAmt = MathHelper.Clamp(MathHelper.Lerp(0f, 8f, (Timer - ShakeThreshold) / (Lifetime - ShakeThreshold)), 0f, 8f);
            Vector2 drawPos = Projectile.Center + Main.rand.NextVector2CircularEdge(shakeAmt, shakeAmt);

            Projectile.DrawProjectileWithBackglow(new Color(255, 255, 25), lightColor, 3.5f, xPos: drawPos.X, yPos: drawPos.Y);
            return false;
        }
    }
}
