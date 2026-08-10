using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class WhitewaterProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/Whitewater";
        public ref float time => ref Projectile.ai[0];

        public bool returning = false;
        public bool empowered = false;
        public bool shattered = false;
        public int shatterTimer = 230;
        public float fade = 0;
        public float damageModifier = 1;
        public float randSize;

        public int returnTime = 500;
        public int becomeEmpoweredTime = 60;

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 800;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10 * Projectile.MaxUpdates;
        }
        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];

            randSize = Main.rand.NextFloat(0.8f, 1.2f);
            Projectile.rotation += 0.02f * Projectile.velocity.Length();

            if (time >= returnTime)
                returning = true;
            if (time > becomeEmpoweredTime && time < returnTime && !shattered)
            {
                fade = MathHelper.Lerp(fade, 1, 0.06f);
                empowered = true;
            }
            else
            {
                fade = MathHelper.Lerp(fade, 0, 0.07f);
                empowered = false;
            }
            if (empowered)
            {
                Lighting.AddLight(Projectile.Center, Color.LightBlue.ToVector3() * fade);
                if (Main.rand.NextBool(5))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(15, 15), DustID.FireworksRGB);
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.3f, 0.55f);
                    dust.velocity = Projectile.velocity * Main.rand.NextFloat(0.2f, 0.4f);
                    dust.color = Color.LightBlue;
                }
            }

            if (time > 20 && !returning && !shattered)
            {
                Vector2 moveToMouse = (Owner.ClampedMouseWorld() - Projectile.Center).SafeNormalize(Vector2.UnitX);
                if (Projectile.velocity.Length() < 12)
                    Projectile.velocity += moveToMouse * ( 1 - (0.85f * Utils.GetLerpValue(returnTime / 2, 0, time, true)));
                else
                    Projectile.velocity *= 0.9f;
            }
            if (shattered && !returning)
            {
                fade = MathHelper.Lerp(fade, 1, 0.07f);
                Projectile.extraUpdates = 5;
                Projectile.velocity = Vector2.Zero;
                Projectile.alpha = 255;
                randSize = Main.rand.NextFloat(1.8f, 2.2f);

                Projectile.timeLeft++;
                time--;

                if (shatterTimer <= 140)
                {
                    if (Main.rand.NextBool(5))
                    {
                        Vector2 vel = (Vector2.One * 19).RotatedByRandom(100) * Main.rand.NextFloat(0.9f, 1.1f);
                        Dust dust = Dust.NewDustPerfect(Projectile.Center + vel * 3, DustID.RainbowTorch);
                        dust.noGravity = true;
                        dust.scale = Main.rand.NextFloat(0.5f, 0.85f);
                        dust.velocity = -vel * Main.rand.NextFloat(0.2f, 0.4f);
                        dust.color = Color.LightBlue;
                    }
                }
                if (shatterTimer == 230)
                {
                    for (int i = 0; i < 25; i++)
                    {
                        Vector2 vel = (Vector2.One * 24).RotatedByRandom(100) * Main.rand.NextFloat(0.9f, 1.1f);
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch);
                        dust.noGravity = true;
                        dust.scale = Main.rand.NextFloat(0.5f, 0.85f);
                        dust.velocity = vel * Main.rand.NextFloat(0.2f, 0.4f);
                        dust.color = Color.LightBlue;
                    }
                }
                shatterTimer--;
                if (shatterTimer <= 0)
                {
                    Projectile.alpha = 0;
                    empowered = false;
                    returning = true;
                }
            }
            if (returning)
            {
                Projectile.extraUpdates = 1;
                Projectile.alpha = 0;
                float returnSpeed = 13;
                float acceleration = 1.2f;
                Vector2 playerCenter = Owner.Center;
                float xDist = playerCenter.X - Projectile.Center.X;
                float yDist = playerCenter.Y - Projectile.Center.Y;
                float dist = (float)Math.Sqrt(xDist * xDist + yDist * yDist);
                dist = returnSpeed / dist;
                xDist *= dist;
                yDist *= dist;

                if (Projectile.velocity.X < xDist)
                {
                    Projectile.velocity.X = Projectile.velocity.X + acceleration;
                    if (Projectile.velocity.X < 0f && xDist > 0f)
                        Projectile.velocity.X += acceleration;
                }
                else if (Projectile.velocity.X > xDist)
                {
                    Projectile.velocity.X = Projectile.velocity.X - acceleration;
                    if (Projectile.velocity.X > 0f && xDist < 0f)
                        Projectile.velocity.X -= acceleration;
                }
                if (Projectile.velocity.Y < yDist)
                {
                    Projectile.velocity.Y = Projectile.velocity.Y + acceleration;
                    if (Projectile.velocity.Y < 0f && yDist > 0f)
                        Projectile.velocity.Y += acceleration;
                }
                else if (Projectile.velocity.Y > yDist)
                {
                    Projectile.velocity.Y = Projectile.velocity.Y - acceleration;
                    if (Projectile.velocity.Y > 0f && yDist < 0f)
                        Projectile.velocity.Y -= acceleration;
                }
                // Delete the projectile if it touches its owner
                if (Main.myPlayer == Projectile.owner)
                {
                    if (Projectile.Hitbox.Intersects(Owner.Hitbox))
                    {
                        if (Projectile.Calamity().stealthStrike)
                        {
                            foreach (Projectile p in Main.ActiveProjectiles)
                            {
                                if (p.type == ModContent.ProjectileType<WhitewaterAura>() && p.owner == Projectile.owner)
                                {
                                    if (p.timeLeft > 30)
                                        p.timeLeft = 30;
                                }
                            }
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<WhitewaterAura>(), (int)(Projectile.damage * 0.25f), Projectile.knockBack, Projectile.owner, 0f, 0f);
                        }
                        Projectile.Kill();
                    }
                }
            }
            time++;
        }
        public override bool? CanDamage() => (shattered || returning) ? false : null;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (damageModifier > 0.05f)
                damageModifier -= 0.05f;

            if (empowered && !shattered)
            {
                SoundStyle sound = new("CalamityMod/Sounds/Item/BreakAndReform");
                SoundEngine.PlaySound(sound with { Volume = 0.4f }, Projectile.Center);

                int points = Projectile.Calamity().stealthStrike ? 6 : 4;
                float radians = MathHelper.TwoPi / points;
                Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f));
                for (int k = 0; k < points; k++)
                {
                    Vector2 velocity = spinningPoint.RotatedBy(radians * k).RotatedBy(Projectile.ai[1] == 1 ? MathHelper.ToRadians(45f) : 0) * 4;

                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, -velocity, ModContent.ProjectileType<WhitewaterSpear>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner, 0f, 0f);
                }
                shattered = true;
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.Calamity().stealthStrike)
                modifiers.SourceDamage *= damageModifier;
            else // if hitting before being empowered it does less damage, otherwise on the empowered hit it does full damage, but both hits are effected by the damageMult
                modifiers.SourceDamage *= (empowered && !returning) ? damageModifier : damageModifier * 0.5f;
        }
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Texture2D rTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
            Texture2D wTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/HalfStar").Value;
            Color drawColor2 = Color.LightBlue;

            Main.EntitySpriteDraw(rTexture, Projectile.Center - Main.screenPosition, null, drawColor2 with { A = 0 } * fade * 0.5f, Projectile.rotation, rTexture.Size() * 0.5f, 0.45f * randSize, SpriteEffects.None, 0);

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);

            for (int k = 0; k < 2; k++)
            {
                float rot = (k == 0 ? MathHelper.ToRadians(90f) : 0) + (Projectile.ai[1] == -1 ? MathHelper.ToRadians(45f) : 0);
                Main.EntitySpriteDraw(wTexture, Projectile.Center - Main.screenPosition, null, drawColor2 with { A = 0 } * fade * 0.4f, rot, wTexture.Size() * 0.5f, 1.15f * randSize, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(wTexture, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 } * fade * 0.4f, rot, wTexture.Size() * 0.5f, 0.68f * randSize, SpriteEffects.None, 0);
            }
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 45, targetHitbox);
    }
}
