using System.IO;
using CalamityMod.NPCs.Yharon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class YharonBulletHellVortex : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public ref float TimeCountdown => ref Projectile.ai[0];
        public int victim = 0;
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
        }

        public override void SetDefaults()
        {
            Projectile.width = 408;
            Projectile.height = 408;
            Projectile.scale = 0.05f;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.timeLeft = 60000;
            Projectile.Calamity().DealsDefenseDamage = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(victim);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            victim = reader.ReadInt32();
        }

        public override void AI()
        {
            if ((Main.npc[(int)Projectile.ai[1]].active && Main.npc[(int)Projectile.ai[1]].type == ModContent.NPCType<Yharon>()) || Main.zenithWorld)
            {
                if (TimeCountdown > 0f)
                {
                    if (TimeCountdown <= 20f)
                        Projectile.scale = MathHelper.Clamp(Projectile.scale - 0.05f, 0f, 1f);
                    else if (Projectile.scale < 1f)
                        Projectile.scale = MathHelper.Clamp(Projectile.scale + 0.05f, 0f, 1f);

                    TimeCountdown--;

                    // chase players in the zenith seed
                    if (Main.zenithWorld)
                    {
                        Projectile.hostile = true;
                        Projectile.width = Projectile.height = (int)(408f * Projectile.scale);
                        float inertia = 5f;
                        float speed = 5.35f;
                        float minDist = 160f;
                        if (victim >= 0 && Main.player[victim].active && !Main.player[victim].dead)
                        {
                            if (Projectile.Distance(Main.player[victim].Center) > minDist)
                            {
                                Vector2 moveDirection = Projectile.SafeDirectionTo(Main.player[victim].Center, Vector2.UnitY);
                                Projectile.velocity = (Projectile.velocity * (inertia - 1f) + moveDirection * speed) / inertia;
                            }
                        }
                        else
                        {
                            victim = Player.FindClosest(Projectile.Center, 1, 1);
                            Projectile.netUpdate = true;
                        }

                        // Fly away from other vortices
                        float pushForce = 0.05f;
                        for (int k = 0; k < Main.maxProjectiles; k++)
                        {
                            Projectile otherProj = Main.projectile[k];
                            // Short circuits to make the loop as fast as possible
                            if (!otherProj.active || k == Projectile.whoAmI)
                                continue;

                            // If the other projectile is indeed the same owned by the same player and they're too close, nudge them away.
                            bool sameProjType = otherProj.type == Projectile.type;
                            float taxicabDist = Vector2.Distance(Projectile.Center, otherProj.Center);
                            float distancegate = 360f;
                            if (sameProjType && taxicabDist < distancegate)
                            {
                                if (Projectile.position.X < otherProj.position.X)
                                    Projectile.velocity.X -= pushForce;
                                else
                                    Projectile.velocity.X += pushForce;

                                if (Projectile.position.Y < otherProj.position.Y)
                                    Projectile.velocity.Y -= pushForce;
                                else
                                    Projectile.velocity.Y += pushForce;
                            }
                        }
                    }
                }
                else
                    Projectile.Kill();
            }
            else
                Projectile.Kill();
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Main.spriteBatch.EnterShaderRegion();
            Texture2D vortexNoise = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Cracks").Value;
            GameShaders.Misc["CalamityMod:DoGPortal"].UseOpacity(1f);
            GameShaders.Misc["CalamityMod:DoGPortal"].UseColor(Color.Gold);
            GameShaders.Misc["CalamityMod:DoGPortal"].UseSecondaryColor(Color.White);
            GameShaders.Misc["CalamityMod:DoGPortal"].Apply();
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f + Main.GlobalTimeWrappedHourly * MathHelper.TwoPi;
                Color drawColor = Color.White;
                drawColor.A = 0;
                Vector2 drawPosition = Projectile.Center - Main.screenPosition + angle.ToRotationVector2() * 3f;
                Main.EntitySpriteDraw(vortexNoise, drawPosition, null, drawColor, angle + MathHelper.PiOver2, vortexNoise.Size() * 0.5f, Projectile.scale * 1.7f, SpriteEffects.None, 0);
            }
            Main.spriteBatch.ExitShaderRegion();
            return false;
        }
    }
}
