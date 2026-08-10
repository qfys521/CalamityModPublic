using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using CalamityMod.Particles;
using Terraria.DataStructures;
using System.Collections.Generic;
using CalamityMod.Graphics.Primitives;
using Terraria.Graphics.Shaders;

namespace CalamityMod.Projectiles.Melee
{
    public class MantisClawJet : ModProjectile, ILocalizedModType
    {
        int TimerCap = 70;

        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Particles/ThunderBolt";

        public static Color WaterColor = new Color(114, 197, 255, 0);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 40;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
            Projectile.timeLeft = TimerCap;
            Projectile.tileCollide = false;
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 5;
            Projectile.DamageType = DamageClass.Melee;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.ai[1] = 40;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.97f;

            if (Projectile.ai[0] < 16) Projectile.ai[1] -= 2;

            Projectile.ai[0]++;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player owner = Main.player[Projectile.owner];
            modifiers.SetCrit();
        }

        public override bool PreDraw(Player player, ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            for (int i = 0; i < 6; i++)
            {
                float ii = ((float)i) / 6f;
                Vector2 position = Vector2.Lerp(Projectile.position, Projectile.oldPos[5], ii);

                float Prog = MathHelper.Lerp(3, 1, (float)Projectile.timeLeft / (float)TimerCap);

                Gore bubble = Gore.NewGorePerfect(Projectile.GetSource_FromAI(), position, Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f) * Prog, 411);
                bubble.timeLeft = 8 + Main.rand.Next(6);
                bubble.scale = Main.rand.NextFloat(0.6f, 1f) * (1 + Prog * 0.4f);
                bubble.type = Main.rand.NextBool(3) ? 412 : 411;
            }

            for (int i = 0; i < 3; i++)
            {
                float ii = ((float)i) / 3f;
                Vector2 position = Vector2.Lerp(Projectile.position, Projectile.oldPos[5], ii);

                float Prog = MathHelper.Lerp(3, 10, (float)Projectile.timeLeft / (float)TimerCap);

                GeneralParticleHandler.SpawnParticle(new WaterFoamParticle(position, Projectile.velocity * 0.4f, 10 - (i * 2), Prog, WaterColor));
            }

            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);

            List<Vector2> positions = new();

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                positions.Add(Projectile.oldPos[i]);
            }

            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>(Texture));

            PrimitiveRenderer.RenderTrail(positions, new PrimitiveSettings((AA,_) => { return Projectile.ai[1]; }, (CC,_) => { return Lighting.GetColor(Vector2.Lerp(Projectile.oldPos[Projectile.oldPos.Length - 1], Projectile.position, CC).ToPoint()).MultiplyRGBA(WaterColor); }, shader: GameShaders.Misc["CalamityMod:TrailStreak"]));

            return false;
        }
    }
}
