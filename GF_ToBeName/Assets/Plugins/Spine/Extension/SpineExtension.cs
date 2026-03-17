using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using static Spine.Skin;

namespace Spine
{
    public static class SpineExtension
    {
        public static void RemoveSkin(this Skin skin, Skin removeSkin)
        {
            foreach (SkinEntry entry in removeSkin.Attachments.Keys)
            {
                skin.RemoveAttachment(entry.SlotIndex, entry.Name);
            }
        }

        public static void SetGray(this Skin skin, bool isGray, Skeleton skeleton, bool fromOther)
        {
            foreach (SkinEntry entry in skin.Attachments.Keys)
            {
                var slotIndex = fromOther ? skeleton.FindSlotIndex(entry.Name) : entry.SlotIndex;
                if (slotIndex >= skeleton.Slots.Items.Length) continue;
                if (slotIndex < 0) continue;
                Slot slot = skeleton.Slots.Items[slotIndex];

                if (isGray)
                {
                    slot.R = 0;
                    slot.Data.R = 0;
                }
                else
                {
                    slot.R = 1;
                    slot.Data.R = 1;
                }
            }
        }
    }
}

