using System.IO;
using CalamityMod.Buffs.Summon;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.Utilities.Daybreak;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Summon
{
    public class SarosAura : ModProjectile, ILocalizedModType
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
            player.AddBuff(ModContent.BuffType<SarosPossessionBuff>(), 3600);

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
                    player.channel = false;
                    Projectile.netUpdate = true;
                }
                if (MinionSlotsToAdd > 0)
                {
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SarosEclipseBeam>(), Projectile.damage * (int)Projectile.minionSlots, Projectile.knockBack, Projectile.owner);
                }
                MinionSlotsToAdd = 0;
            }
            #endregion

            #region Checking alive
            bool correctMinion = Projectile.type == ModContent.ProjectileType<SarosAura>();
            if (correctMinion)
            {
                if (player.dead)
                {
                    modPlayer.sunSpirit = false;
                }
                if (modPlayer.saros)
                {
                    Projectile.timeLeft = 2;
                }
            }
            #endregion

            #region Positioning
            Projectile.Center = player.Center + Vector2.UnitY * (player.gfxOffY + player.gravDir * -24f);
            #endregion

            Lighting.AddLight(Projectile.Center, (255 - Projectile.alpha) * 0.25f / 255f, (255 - Projectile.alpha) * 0.25f / 255f, (255 - Projectile.alpha) * 0f / 255f);

            NPC target = GetTargetInRange(1600);

            if (target is null)
                return;


            if (Projectile.owner == Main.myPlayer)
            {

                if (Projectile.ai[1] > 0f)
                {
                    Projectile.ai[1] -= 1f;
                }
                else
                {
                    if (player.ownedProjectileCounts[ModContent.ProjectileType<SarosEclipseBeam>()] <= 0)
                    {
                        int damage = (int)(Projectile.damage * (0.8f + Projectile.minionSlots * 0.2f));
                        float shootSpeed = 15f;
                        Vector2 source = Projectile.Center;
                        for (var i = 0; i < 3; i++)
                        {
                            SoundEngine.PlaySound(SarosPossession.FiringSound, Projectile.Center);
                            var velocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * shootSpeed;
                            Projectile beam = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center - velocity, velocity, ModContent.ProjectileType<SarosSunfire>(), damage, Projectile.knockBack, Projectile.owner, 120, ai1: (Projectile.minionSlots - 1) / 9f);
                            beam.DamageType = DamageClass.Summon;
                        }
                        Projectile.ai[1] += 60f / (0.8f + Projectile.minionSlots * 0.2f);
                    }
                }

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
        NPC GetTargetInRange(float range)
        {
            var player = Main.player[Projectile.owner];
            if (player.HasMinionAttackTargetNPC && Main.npc[player.MinionAttackTargetNPC].CanBeChasedBy() && Projectile.IsInRangeOfMeOrMyOwner(Main.npc[player.MinionAttackTargetNPC], range, out var _, out var _, out var _))
            {
                return Main.npc[player.MinionAttackTargetNPC];
            }
            else
            {
                NPC gotTarget = null;
                float currentDistance = range;
                foreach (var npc in Main.ActiveNPCs)
                {
                    var myDistance = npc.Distance(Projectile.Center);
                    if (npc.CanBeChasedBy() && myDistance < currentDistance)
                    {
                        currentDistance = myDistance;
                        gotTarget = npc;
                    }
                }
                return gotTarget;
            }
        }

        public override bool? CanDamage() => false;

        public static Asset<Texture2D> circle;
        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            //Sigil
            var spTex = TextureAssets.Projectile[Type].Value;
            Main.spriteBatch.Draw(spTex, Projectile.Center - Main.screenPosition, null, Color.White, Main.GlobalTimeWrappedHourly, spTex.Size() * 0.5f, 0.75f, SpriteEffects.None, 0);
            
            using (Main.spriteBatch.Scope())
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                //Sunlines
                int count = (int)((Projectile.minionSlots - 1) / 3) * 2 + 2;
                for (var i = 0; i < count; i++)
                {
                    var comp = (i / (float)count);
                    var offset = ((Main.mouseTextColor - 190) / 64f) * 8;
                    if (i % 2 == 0)
                        offset = 8 - offset;
                    CalamityUtils.DrawLineBetter(Main.spriteBatch, Projectile.Center + new Vector2(20 + offset, 0).RotatedBy(MathHelper.TwoPi * comp - Main.GlobalTimeWrappedHourly), Projectile.Center + new Vector2(34 + offset, 0).RotatedBy(MathHelper.TwoPi * comp - Main.GlobalTimeWrappedHourly), i % 2 == 3 ? Color.OrangeRed : Color.Gold, 2f);
                }

                //Circles
                var ciTex = CalamityUtils.GetTextureEfficient(ref circle, "CalamityMod/Particles/BloomRingThinLarge").Value;
                count = (int)MathHelper.Min((1 + (Projectile.minionSlots - 1) % 3), 10);
                for (var i = 0; i < count && i < 5; i++)
                {
                    var comp = (i / (count));
                    var offset = ((Main.mouseTextColor - 190) / 64f);
                    if (i % 2 == 0)
                        offset = 1 - offset;
                    Main.EntitySpriteDraw(ciTex, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Gold, Color.OrangeRed, i / (4f)), Main.GlobalTimeWrappedHourly, ciTex.Size() * 0.5f, 0.0225f + 0.0025f * offset + 0.005f * i, SpriteEffects.None);
                }

                Main.spriteBatch.End();
            }
            return false;
        }
    }
}
