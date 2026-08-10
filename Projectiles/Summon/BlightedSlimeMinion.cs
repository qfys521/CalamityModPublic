using CalamityMod.Buffs.Summon;
using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class BlightedSlimeMinion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public float dust = 0f;

        public static Asset<Texture2D> Corroslime;
        public static Asset<Texture2D> Crimslime;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargetingFeature[Type] = true;

            if (!Main.dedServ)
            {
                Corroslime = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Summon/BlightedSlimeCorro");
                Crimslime = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Summon/BlightedSlimeCrim");
            }
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.minionSlots = 1;
            Projectile.alpha = 75;
            Projectile.aiStyle = ProjAIStyleID.Pet;
            Projectile.timeLeft = 18000;
            Projectile.penetrate = -1;
            Projectile.timeLeft *= 5;
            Projectile.minion = true;
            AIType = ProjectileID.BabySlime;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            CalamityPlayer modPlayer = player.Calamity();
            if (dust == 0f)
            {
                int dustAmt = 16;
                for (int i = 0; i < dustAmt; i++)
                {
                    Vector2 rotate = Vector2.Normalize(Projectile.velocity) * new Vector2(Projectile.width / 2f, Projectile.height) * 0.75f;
                    rotate = rotate.RotatedBy((i - (dustAmt / 2 - 1)) * MathHelper.TwoPi / (float)dustAmt) + Projectile.Center;
                    Vector2 faceDirection = rotate - Projectile.Center;
                    int dust = Dust.NewDust(rotate + faceDirection, 0, 0, WorldGen.crimson ? DustID.Blood : DustID.Demonite, faceDirection.X, faceDirection.Y, 100, default, 1.1f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = faceDirection;
                }
                dust += 1f;
            }
            bool isMinion = Projectile.type == ModContent.ProjectileType<BlightedSlimeMinion>();
            player.AddBuff(ModContent.BuffType<BlightedSlime>(), 3600);
            if (isMinion)
            {
                if (player.dead)
                {
                    modPlayer.blightedSlime = false;
                }
                if (modPlayer.blightedSlime)
                {
                    Projectile.timeLeft = 2;
                }
            }
        }

        public override bool MinionContactDamage() => true;

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D texture2D13 = (WorldGen.crimson ? Crimslime : Corroslime).Value;
            int framing = texture2D13.Height / Main.projFrames[Type];
            int y6 = framing * Projectile.frame;
            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, y6, texture2D13.Width, framing)), Projectile.GetAlpha(lightColor), Projectile.rotation, new Vector2((float)texture2D13.Width / 2f, (float)framing / 2f), Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => false;
    }
}
