using System.IO;
using CalamityMod.Buffs.Summon;
using CalamityMod.CalPlayer;
using CalamityMod.Utilities.Daybreak;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Summon
{
    public class SunSpiritMinion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargetingFeature[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 1f;
            Projectile.timeLeft = 18000;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft *= 5;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
        }

        public int MinionSlotsToAdd
        {
            get { return (int)Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            CalamityPlayer modPlayer = player.Calamity();
            player.AddBuff(ModContent.BuffType<SolarSpirit>(), 3600);

            #region Add Minion Slots
            if (MinionSlotsToAdd > 0)
            {
                float minionSlotsAvaliable = player.maxMinions;
                foreach (var item in Main.ActiveProjectiles)
                {
                    if (item.owner == Projectile.owner)
                        minionSlotsAvaliable -= item.minionSlots;
                }
                while (minionSlotsAvaliable >= 1 && MinionSlotsToAdd > 0)
                {

                    Projectile.minionSlots++;
                    minionSlotsAvaliable--;
                    MinionSlotsToAdd--;
                    Projectile.netUpdate = true;
                }
                MinionSlotsToAdd = 0;
            }
            #endregion

            #region Checking alive
            bool correctMinion = Projectile.type == ModContent.ProjectileType<SunSpiritMinion>();
            if (correctMinion)
            {
                if (player.dead)
                {
                    modPlayer.sunSpirit = false;
                }
                if (modPlayer.sunSpirit)
                {
                    Projectile.timeLeft = 2;
                }
            }
            #endregion

            #region Positioning
            Projectile.Center = player.Center + Vector2.UnitY * (player.gfxOffY + player.gravDir * -80f);
            #endregion

            Lighting.AddLight(Projectile.Center, (255 - Projectile.alpha) * 0.25f / 255f, (255 - Projectile.alpha) * 0.25f / 255f, (255 - Projectile.alpha) * 0f / 255f);

            NPC target = null;
            int targetID = -1;
            Projectile.Minion_FindTargetInRange(800, ref targetID, false);
            if (targetID < 0)
                return;
            
            target = Main.npc[targetID];


            if (Projectile.owner == Main.myPlayer)
            {
                if (Projectile.ai[1] > 0f)
                {
                    Projectile.ai[1] -= 1f;
                    return;
                }
                float shootSpeed = 15f;
                Vector2 source = Projectile.Center;
                var velocity = CalamityUtils.CalculatePredictiveAimToTargetMaxUpdates(Projectile.Center, target, shootSpeed, 2);
                Projectile beam = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center - velocity, velocity, ModContent.ProjectileType<SunSpiritBeam>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                beam.DamageType = DamageClass.Summon;
                Projectile.ai[1] += 50f / Projectile.minionSlots;
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.minionSlots);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.minionSlots = reader.ReadSingle();
        }

        public override bool? CanDamage() => false;

        public static Asset<Texture2D> circle;
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            var spTex = TextureAssets.Projectile[Type].Value;
            var ciTex = CalamityUtils.GetTextureEfficient(ref circle, "CalamityMod/ExtraTextures/GreyscaleOpenCircleButBigger").Value;

            Main.spriteBatch.Draw(spTex, Projectile.Center - Main.screenPosition, null, Color.White, Main.GlobalTimeWrappedHourly, spTex.Size() * 0.5f, 0.75f, SpriteEffects.None, 0);

            using (Main.spriteBatch.Scope())
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                float count = MathHelper.Min(Projectile.minionSlots * 2, 40);
                for (var i = 0; i < count; i++)
                {
                    var comp = (i / count);
                    var offset = ((Main.mouseTextColor - 190) / 64f) * 8;
                    if (i % 2 == 0)
                        offset = 8 - offset;
                    CalamityUtils.DrawLineBetter(Main.spriteBatch, Projectile.Center + new Vector2(20 + offset, 0).RotatedBy(MathHelper.TwoPi * comp - Main.GlobalTimeWrappedHourly), Projectile.Center + new Vector2(34 + offset, 0).RotatedBy(MathHelper.TwoPi * comp - Main.GlobalTimeWrappedHourly), i % 2 == 3 ? Color.OrangeRed : Color.Gold, 2f);
                }
                Main.spriteBatch.End();
            }
            return false;
        }
    }
}
