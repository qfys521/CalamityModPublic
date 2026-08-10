using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Typeless
{
    public class RiftReelerBobber : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = ProjAIStyleID.Bobber;
            Projectile.bobber = true;
            Projectile.friendly = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.penetrate = -1;
            Projectile.ArmorPenetration = 100;
        }

        public override bool PreDrawExtras(Player player)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            if (Projectile.ai[2] == 0f)
                Lighting.AddLight(Projectile.Center, 0.5f, 0.25f, 0f);
            else
                Lighting.AddLight(Projectile.Center, 0f, 0.45f, 0.46f);
            return true;
        }
    }
}
