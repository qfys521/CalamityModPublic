using CalamityMod.Items.Weapons.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class EarthenTidesBlast : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Projectiles/Magic/PrimordialAncientProjectile";
        public Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults() => Main.projFrames[Type] = 6;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 140;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 72;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 40;
            Projectile.scale = 0.05f;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.9f, 0.45f, 0.05f);
            Projectile.rotation = Projectile.ai[0];
            if (Projectile.timeLeft > 12)
            {
                if (Projectile.scale < Projectile.ai[1])
                    Projectile.scale += (Projectile.ai[1] - 0.05f) / 12f;
            }
            else
                Projectile.scale -= (Projectile.ai[1] - 0.05f) / 12f;

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 2)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.frame = 0;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Owner.HeldItem.ModItem is OmegaBiomeBlade sword && Main.rand.NextFloat() <= OmegaBiomeBlade.ShockwaveAttunement_BlastProc)
                sword.OnHitProc = true;
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, frame.Size() / 2f, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
