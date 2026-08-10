using System;
using CalamityMod.Items.Weapons.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Melee
{
    public class EarthenTidesBeam : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public NPC target;
        public Player Owner => Main.player[Projectile.owner];
        public override string Texture => "CalamityMod/Projectiles/Melee/TrueBiomeBlade_EarthenTidesBeam";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 2;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.aiStyle = ProjAIStyleID.Beam;
            AIType = ProjectileID.LightBeam;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.extraUpdates = 1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            // Sword beams home
            if (Projectile.timeLeft < 130)
            {
                int index = -1;
                // Prioritize an enemy that is marked
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.type == ProjectileType<PurityProjectionSigil>() && proj.owner == Owner.whoAmI)
                    {
                        if (Vector2.Distance(Main.npc[(int)proj.ai[0]].Center, Projectile.Center) < 560f)
                        {
                            index = (int)proj.ai[0];
                            break;
                        }
                    }
                }

                // If nothing is marked, just find the closest target
                if (index == -1)
                {
                    float npcDistCompare = 240f;
                    foreach (NPC n in Main.ActiveNPCs)
                    {
                        if (!n.CanBeChasedBy(Projectile, false))
                            continue;

                        float currentNPCDist = Vector2.Distance(n.Center, Projectile.Center);
                        if ((currentNPCDist < npcDistCompare) && Collision.CanHit(Projectile.Center, 1, 1, n.Center, 1, 1))
                        {
                            npcDistCompare = currentNPCDist;
                            index = n.whoAmI;
                        }
                    }
                }

                // If we found an enemy, curve towards them
                if (index != -1)
                {
                    float homingTurnSpeed = 0.05f;
                    Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(Projectile.SafeDirectionTo(Main.npc[index].Center).ToRotation(), homingTurnSpeed).ToRotationVector2() * 10f;
                }
            }

            Lighting.AddLight(Projectile.Center, 0.75f, 1f, 0.24f);
            int dustParticle = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CursedTorch, 0f, 0f, 100, default, 0.9f);
            Main.dust[dustParticle].noGravity = true;
            Main.dust[dustParticle].velocity *= 0.5f;
            Main.dust[dustParticle].velocity += Projectile.velocity * 0.1f;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (Projectile.timeLeft > 145)
                return false;

            DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);

            Main.spriteBatch.End(); //Yippee wahoo spritebatch restarting!
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.HotPink, Color.GreenYellow, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f)), Projectile.rotation, tex.Size() * 0.5f, 1f, 0f, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item43, Projectile.Center);
            for (int i = 0; i <= 15; i++)
            {
                Vector2 displace = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2() * (-0.5f + (i / 15f)) * 88f;
                int dustParticle = Dust.NewDust(Projectile.Center + displace, Projectile.width, Projectile.height, DustID.CursedTorch, 0f, 0f, 100, default, 2f);
                Main.dust[dustParticle].noGravity = true;
                Main.dust[dustParticle].velocity = Projectile.oldVelocity;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Owner.HeldItem.ModItem is OmegaBiomeBlade sword && Main.rand.NextFloat() <= OmegaBiomeBlade.ShockwaveAttunement_SwordBeamProc)
                sword.OnHitProc = true;
        }

        public override Color? GetAlpha(Color lightColor) => Color.Lerp(Color.DeepSkyBlue, Color.GreenYellow, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f));
    }
}
