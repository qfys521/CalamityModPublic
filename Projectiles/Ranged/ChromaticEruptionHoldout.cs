using System.IO;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class ChromaticEruptionHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<ChromaticEruption>();
        public override string Texture => "CalamityMod/Projectiles/Ranged/ChromaticEruptionHoldout";
        public override float MaxOffsetLengthFromArm => 40f;
        public override float OffsetXUpwards => -10f;
        public override float BaseOffsetY => -12f;
        public override float OffsetYDownwards => 10f;
        public override Vector2 GunTipPosition => Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * Projectile.width * 0.45f;

        public ref float ShotCooldown => ref Projectile.ai[0];
        public ref float ShotsFired => ref Projectile.ai[1];
        public ref float ShootTimer => ref Projectile.ai[2];
        public int FireBlobs { get; set; }

        public override void KillHoldoutLogic()
        {
            base.KillHoldoutLogic();
            if (ShotsFired >= 24)
                Projectile.Kill();
        }

        public override void HoldoutAI()
        {
            var effectcolor = Main.rand.Next(4) switch
            {
                0 => Color.DeepSkyBlue,
                1 => Color.MediumSpringGreen,
                2 => Color.DarkOrange,
                _ => Color.Violet,
            };

            ShootTimer++;
            if (ShootTimer >= 60)
            {
                if (ShotCooldown == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item34, Projectile.Center);
                    Owner.PickAmmo(Owner.HeldItem, out _, out float shootSpeed, out int damage, out float knockback, out _);
                    if (Main.myPlayer == Projectile.owner)
                    {
                        for (int i = 0; i < 2; i++)
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, (Projectile.velocity * 10).RotatedByRandom(0.12f), ModContent.ProjectileType<ChromaticFire>(), damage, knockback, Projectile.owner);
                    }

                    ShotsFired++;
                    ShotCooldown = HeldItem.useTime;
                    if (FireBlobs == 0 && Main.myPlayer == Projectile.owner)
                    {
                        Vector2 newVel = (Projectile.velocity * 9);
                        Vector2 newPos = GunTipPosition + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 36f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), newPos, newVel, ModContent.ProjectileType<ChromaticFlare>(), damage, knockback, Projectile.owner, newVel.Length(), -1f);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), newPos, newVel, ModContent.ProjectileType<ChromaticFlare>(), damage, knockback, Projectile.owner, newVel.Length(), 1f);
                        FireBlobs = 3;
                    }
                    else
                        FireBlobs--;
                }
                else
                    ShotCooldown--;
            }
            else
            {
                ShotsFired = 0;
                for (int i = 0; i < 2; i++)
                {
                    int dustType = Main.rand.NextBool() ? 66 : 247;
                    float rotMulti = Main.rand.NextFloat(0.3f, 1f);
                    Dust dust = Dust.NewDustPerfect(GunTipPosition, dustType);
                    dust.scale = Main.rand.NextFloat(1.2f, 1.8f) * (ShootTimer * 0.025f) - rotMulti * 0.1f;
                    dust.noGravity = true;
                    dust.velocity = new Vector2(0, -2).RotatedByRandom(rotMulti * 0.3f) * (Main.rand.NextFloat(1f, 3.2f) - rotMulti) * (ShootTimer * 0.025f);
                    dust.alpha = Main.rand.Next(90, 150);
                    dust.color = effectcolor;
                }
            }
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            SoundEngine.PlaySound(SoundID.Item73 with { Volume = 0.7f }, Projectile.Center);
        }

        public override void SendExtraAIHoldout(BinaryWriter writer) => writer.Write(FireBlobs);

        public override void ReceiveExtraAIHoldout(BinaryReader reader) => FireBlobs = reader.ReadInt32();
        public override void PostDraw(Player player, Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipVertically;

            Main.EntitySpriteDraw(ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/ChromaticEruptionHoldoutGlow").Value, Projectile.Center - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, 0, texture.Width, texture.Height)), Color.White, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
        }
    }
}
