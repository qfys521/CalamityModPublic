using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Typeless
{
    public class VerstaltiteBobber : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = ProjAIStyleID.Bobber;
            Projectile.bobber = true;
        }

        public override bool PreDrawExtras(Player player)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */
        {
            Lighting.AddLight(Projectile.Center, 0.4f, 0f, 0.4f);
            return true;
        }
    }
}
