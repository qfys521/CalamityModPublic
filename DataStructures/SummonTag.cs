using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;

namespace CalamityMod.DataStructures
{
    /// <summary>
    /// This is used to store summon tag stats so they can be applied automatically.
    /// </summary>
    public class SummonTag
    {
        #region SummonTag data
        /// <summary>
        /// Flat tag damage from this summon tag
        /// </summary>
        public int FlatTagDamage = 0;

        /// <summary>
        /// This is tag damage that isn't added to the tooltip. Used for removing Vanilla whip tag damage.
        /// </summary>
        public int UnlistedTagDamage = 0;

        /// <summary>
        /// Multiplicative tag damage from this summon tag. Stacks additively with other tags.
        /// </summary>
        public float MultiplicativeTagDamage = 0;

        /// <summary>
        /// This is tag damage that isn't added to the tooltip. Used for removing Firecracker whip damage.
        /// </summary>
        public float UnlistedMultiplicativeTagDamage = 0;

        /// <summary>
        /// Tag critical strike chance from this stummon tag. Stacks additively with other tags.
        /// </summary>
        public float TagCritChance = 0;

        /// <summary>
        /// How much the Critical Strike damage multiplier should be adjusted with this tag. Stacks additively with other tags.
        /// </summary>
        public float TagCritDamage = 0;

        /// <summary>
        /// UNIMPLEMENTED. Need to add animation frame support before using.
        /// What the texture for the tag effect should be.
        /// </summary>
        public Asset<Texture2D> TagTexture;

        /// <summary>
        /// What item inflicted this tag
        /// </summary>
        public int TagItem = -1;

        /// <summary>
        /// The ModifyHitByProjectileTag code to run. Defaults to just applying the tag effects.
        /// </summary>
        public ModifyHitByProjectileTag TagModifyHitEffects;

        /// <summary>
        /// Disable to stop tooltips from drawing automatically, for things that change TagModifyHitEffects
        /// </summary>
        public bool AutoDrawTooltip = true;

        /// <summary>
        /// Setting to true allows this tag to be used alongside other tag effects.
        /// </summary>
        public bool AllowsWhipStacking = false;

        /// <summary>
        /// The OnHitByProjectile code to run when this tag is applied. Defaults to nothing.
        /// </summary>
        public OnHitByProjectile TagOnHit = BlankOnHit;
        public SummonTag()
        {
            TagModifyHitEffects = ApplyTagModifyHit;
        }

        public SummonTag(int itemType)
        {
            TagItem = itemType;
            TagModifyHitEffects = ApplyTagModifyHit;
        }

        /// <summary>
        /// A modified version of ModifyHitByProjectile that incorporates tag damage multipliers and tag critical chance.
        /// This delegate is used for TagModifyHitEffects
        /// </summary>
        /// <param name="proj"></param>
        /// <param name="npc"></param>
        /// <param name="modifiers"></param>
        /// <param name="tagDamageMult"></param>
        /// <param name="critChance"></param>
        public delegate void ModifyHitByProjectileTag(Projectile proj, NPC npc, ref NPC.HitModifiers modifiers, ref float tagDamageMult, ref float critChance);

        /// <summary>
        /// This delegate is used for TagOnHit
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="projectile"></param>
        /// <param name="hit"></param>
        /// <param name="damageDone"></param>
        public delegate void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone);

        /// <summary>
        /// The default tag damage functionality
        /// </summary>
        public void ApplyTagModifyHit(Projectile proj, NPC npc, ref NPC.HitModifiers modifiers, ref float tagDamageMult, ref float critChance)
        {
            modifiers.FlatBonusDamage += (FlatTagDamage + UnlistedTagDamage) * tagDamageMult;
            modifiers.ScalingBonusDamage += (MultiplicativeTagDamage + UnlistedMultiplicativeTagDamage) * tagDamageMult;
            critChance += TagCritChance;
            modifiers.CritDamage += TagCritDamage;
        }

        /// <summary>
        /// Blank tag damage functionality
        /// </summary>
        public static void BlankTagModifyHit(Projectile proj, NPC npc, ref NPC.HitModifiers modifiers, ref float tagDamageMult, ref float critChance)
        {
        }
        /// <summary>
        /// This is replaced by custom tag on hit effects. Does nothing.
        /// </summary>
        public static void BlankOnHit(NPC npc, Projectile projectile, NPC.HitInfo hit, int damagedone) { }
        #endregion

        #region Vanilla whip stats
        public static SummonTag LeatherWhip = new SummonTag(ItemID.BlandWhip)
        {
            FlatTagDamage = 2,
            UnlistedTagDamage = -4 //Negative value cancels out vanilla tag damage
        };
        public static SummonTag Snapthorn = new SummonTag(ItemID.ThornWhip)
        {
            FlatTagDamage = 1,
            UnlistedTagDamage = -6 //Negative value cancels out vanilla tag damage
        };
        public static SummonTag SpinalTap = new SummonTag(ItemID.BoneWhip)
        {
            FlatTagDamage = 4,
            UnlistedTagDamage = -7 //Negative value cancels out vanilla tag damage
        };
        public static SummonTag Firecracker = new SummonTag(ItemID.FireWhip)
        {
            TagCritChance = 1f, //Since we make firecracker do 2x damage, I make this apply as a crit for the ability to proc crit accessories and for visual style points
            UnlistedMultiplicativeTagDamage = -1.75f, //Cancels out the vanilla firecracker tag damage
            AutoDrawTooltip = false
        };
        public static SummonTag CoolWhip = new SummonTag(ItemID.CoolWhip)
        {
            FlatTagDamage = 6,
            UnlistedTagDamage = -6 //Negative value cancels out vanilla tag damage
        };
        public static SummonTag Durendal = new SummonTag(ItemID.SwordWhip)
        {
            FlatTagDamage = 4,
            TagCritChance = 0.05f,
            UnlistedTagDamage = -9 //Negative value cancels out vanilla tag damage
        };
        public static SummonTag MorningStar = new SummonTag(ItemID.MaceWhip)
        {
            TagCritChance = 0.13f,
            UnlistedTagDamage = -8 //Negative value cancels out vanilla tag damage
        };
        public static SummonTag DarkHarvest = new SummonTag(ItemID.ScytheWhip)
        {
            FlatTagDamage = 10,
            UnlistedTagDamage = -15, // Negative value cancels 1.4.5's vanilla tag damage.
            //This has a unique on hit effect from vanilla.
        };
        public static SummonTag Kaleidoscope = new SummonTag(ItemID.RainbowWhip)
        {
            FlatTagDamage = 8,
            TagCritChance = 0.10f,
            UnlistedTagDamage = -20 //Negative value cancels out vanilla tag damage
        };
        #endregion
    }
}
