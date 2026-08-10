using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Typeless
{
    public class NavyBobber : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = ProjAIStyleID.Bobber;
            Projectile.bobber = true;
        }

        public override void PostAI()
        {
            foreach (var item in Main.ActiveNPCs)
            {
                if (!item.friendly && item.Distance(Projectile.Center) < 160)
                {
                    item.AddBuff(ModContent.BuffType<StaticDischarge>(), 180);
                    if (Main.rand.NextBool(10))
                    {
                        Vector2 velocity = CalamityUtils.RandomVelocity(50f, 30f, 60f);
                        Projectile spark = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), item.Center, velocity, ModContent.ProjectileType<GenericElectricSpark>(), 0, 0f, Projectile.owner);
                        spark.localNPCHitCooldown = -2;
                        spark.timeLeft = 30;
                    }
                }
            }
        }
        public override bool PreDrawExtras(Player player)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Lighting.AddLight(Projectile.Center, 0f, 0.25f, 0.25f);
            return true;
        }
    }
}
