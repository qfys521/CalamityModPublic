using System.IO;
using System.Linq;
using CalamityMod.UI.DialogueDisplay.DisplayEffects;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;
using static CalamityMod.UI.DialogueDisplay.DialogueDisplaySystem;

namespace CalamityMod.Packets
{
    internal sealed class StartDialogueDisplayPacket : CalamityPacket
    {
        public static StartDialogueDisplayPacket Instance { get; private set; }

        public static void Send(string name, bool progressDialogue, Vector2 position, int index, int uptime, DisplayEffectID effect, float wrapWidth, int toClient = -1, int ignoreClient = -1)
        {
            // Only Server should send Reponse to Clients
            if (!Main.dedServ)
                return;

            var packet = Instance.CreateBasePacket();

            packet.Write(name);
            packet.WriteFlags(progressDialogue, false);
            packet.WritePackedVector2(position);
            packet.Write(index);
            packet.Write(uptime);
            packet.Write((byte)effect);
            packet.Write(wrapWidth);

            packet.Send(toClient, ignoreClient);
        }

        public enum EntityType : byte
        {
            NPC,
            Player,
            Projectile
        }

        public static void Send(string name, bool progressDialogue, EntityType type, int entity, int index, int uptime, DisplayEffectID effect, float wrapWidth, int toClient = -1, int ignoreClient = -1)
        {
            // Only Server should send Reponse to Clients
            if (!Main.dedServ)
                return;

            var packet = Instance.CreateBasePacket();

            packet.Write(name);
            packet.WriteFlags(progressDialogue, true);
            packet.Write((byte)type);
            packet.Write(entity);
            packet.Write(index);
            packet.Write(uptime);
            packet.Write((byte)effect);
            packet.Write(wrapWidth);

            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(BinaryReader packet, int sender)
        {
            // Only receive info as clients

            string name = packet.ReadString();
            packet.ReadFlags(out bool progressDialogue, out bool hasEntity);

            int entity = -1;
            Vector2 pos = Vector2.Zero;
            EntityType type = EntityType.NPC;
            if (hasEntity)
            {
                type = (EntityType)packet.ReadByte();
                entity = packet.ReadInt32();
            }
            else
                pos = packet.ReadPackedVector2();

            int index = packet.ReadInt32();
            int uptime = packet.ReadInt32();
            byte effect = packet.ReadByte();
            float wrapWidth = packet.ReadSingle();

            if (Main.netMode != NetmodeID.MultiplayerClient)
                return;

            DisplayEffect de = GetEffect((DisplayEffectID)effect);

            if (hasEntity)
            {
                Entity e = type switch
                {
                    EntityType.NPC => Main.npc[entity],
                    EntityType.Player => Main.player[entity],
                    EntityType.Projectile => Main.projectile.FirstOrDefault(p => p.identity == entity),
                    _ => null
                };

                StartDialogueOnClient(name, e, index, uptime, progressDialogue, de, wrapWidth);
            }
            else
                StartDialogueOnClient(name, pos, index, uptime, progressDialogue, de, wrapWidth);

        }
    }

}
