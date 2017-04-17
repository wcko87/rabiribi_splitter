using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace rabi_splitter_WPF
{
    struct BossStats
    {
        public int entityArrayIndex;
        public int id;
        public Boss type;
        public int hp;
        public int maxHp;
    }

    class MemorySnapshot
    {
        public readonly int t_playtime;
        public readonly int playtime;
        public readonly int blackness;
        public readonly int mapid;
        public readonly int musicid;
        public readonly int money;
        public readonly int hp;
        public readonly int maxhp;

        public readonly int currentSprite;
        public readonly int actionFrame;
        public readonly int animationFrame;

        public readonly float amulet;
        public readonly int boost;
        public readonly float mana;
        public readonly int stamina;

        public readonly float px;
        public readonly float py;

        public readonly int entityArrayPtr;
        public readonly int entityArraySize;
        public readonly int nActiveEntities;
        public readonly List<BossStats> bossList;

        public readonly int carrotXp;
        public readonly int hammerXp;
        public readonly int ribbonXp;
        public readonly float itemPercent;

        public readonly int nAttackUps;
        public readonly int nHpUps;
        public readonly int nManaUps;
        public readonly int nPackUps;
        public readonly int nRegenUps;


        public MemorySnapshot(Process process, int veridx)
        {
            t_playtime = MemoryHelper.GetMemoryValue<int>(process, StaticData.IGTAddr[veridx]);
            playtime = MemoryHelper.GetMemoryValue<int>(process, StaticData.PlaytimeAddr[veridx]);
            blackness = MemoryHelper.GetMemoryValue<int>(process, StaticData.BlacknessAddr[veridx]);

            mapid = MemoryHelper.GetMemoryValue<int>(process, StaticData.MapAddress[veridx]);
            musicid = MemoryHelper.GetMemoryValue<int>(process, StaticData.MusicAddr[veridx]);
            money = MemoryHelper.GetMemoryValue<int>(process, StaticData.MoneyAddress[veridx]);

            carrotXp = MemoryHelper.GetMemoryValue<int>(process, 0xD654BC);
            hammerXp = MemoryHelper.GetMemoryValue<int>(process, 0xD654B4);
            ribbonXp = MemoryHelper.GetMemoryValue<int>(process, 0xD654B8);
            itemPercent = MemoryHelper.GetMemoryValue<int>(process, 0xA730E8);

            nAttackUps = countItems(process, 0xD6352C, 0xD63628);
            nHpUps = countItems(process, 0xD6342C, 0xD63528);
            nManaUps = countItems(process, 0xD6362C, 0xD63728);
            nPackUps = countItems(process, 0xD6382C, 0xD63928);
            nRegenUps = countItems(process, 0xD6372C, 0xD63828);

            entityArrayPtr = MemoryHelper.GetMemoryValue<int>(process, StaticData.EnenyPtrAddr[veridx]);

            hp = MemoryHelper.GetMemoryValue<int>(process, entityArrayPtr + 0x4D8, false);
            maxhp = MemoryHelper.GetMemoryValue<int>(process, entityArrayPtr + 0x4E8, false);

            currentSprite = MemoryHelper.GetMemoryValue<int>(process, entityArrayPtr + 0x654, false);
            actionFrame = MemoryHelper.GetMemoryValue<int>(process, entityArrayPtr + 0x660, false);
            animationFrame = MemoryHelper.GetMemoryValue<int>(process, entityArrayPtr + 0x67c, false);

            amulet = MemoryHelper.GetMemoryValue<float>(process, entityArrayPtr + 0x52C, false);
            boost = MemoryHelper.GetMemoryValue<int>(process, entityArrayPtr + 0x5DC, false);
            mana = MemoryHelper.GetMemoryValue<float>(process, entityArrayPtr + 0x6B8, false);
            stamina = MemoryHelper.GetMemoryValue<int>(process, entityArrayPtr + 0x5B4, false);

            px = MemoryHelper.GetMemoryValue<float>(process, entityArrayPtr + 0xC, false);
            py = MemoryHelper.GetMemoryValue<float>(process, entityArrayPtr + 0x10, false);
            

            // Read Entity Array and Search for boss data
            bossList = new List<BossStats>();
            nActiveEntities = 0;
            entityArraySize = 4;
            int entitySize = StaticData.EnenyEntitySize[veridx];
            int currArrayPtr = entityArrayPtr + entitySize * 4;
            for (int i=0; i<500; ++i) {
                // (Hard limit of reading 500 entries)
                int entityId = MemoryHelper.GetMemoryValue<int>(process,
                    currArrayPtr + StaticData.EnenyEnitiyIDOffset[veridx], false);
                int entityMaxHp = MemoryHelper.GetMemoryValue<int>(process,
                    currArrayPtr + StaticData.EnenyEnitiyMaxHPOffset[veridx], false);

                if (entityId == 0 && entityMaxHp == 0) break;

                int activeFlag = MemoryHelper.GetMemoryValue<int>(process,
                    currArrayPtr + StaticData.EnenyEnitiyIsActiveOffset[veridx], false);
                int animationState = MemoryHelper.GetMemoryValue<int>(process,
                    currArrayPtr + StaticData.EnenyEnitiyAnimationOffset[veridx], false);

                bool isAlive = activeFlag == 1 && animationState >= 0;
                
                if (isAlive && StaticData.IsBoss(entityId))
                {
                    BossStats boss;
                    boss.entityArrayIndex = entityArraySize;
                    boss.id = entityId;
                    boss.hp = MemoryHelper.GetMemoryValue<int>(process, currArrayPtr + StaticData.EnenyEnitiyHPOffset[veridx], false);
                    boss.type = StaticData.GetBoss(entityId).Value;
                    boss.maxHp = entityMaxHp;

                    bossList.Add(boss);
                }

                currArrayPtr += entitySize;

                if (isAlive) ++nActiveEntities;
                ++entityArraySize;
            }
        }

        private int countItems(Process process, int addrFirst, int addrLast)
        {
            int count = 0;
            for (int addr = addrFirst; addr <= addrLast; ++addr)
            {
                count += MemoryHelper.GetMemoryValue<int>(process, addr) == 1 ? 1 : 0;
            }
            return count;
        }
        
        public bool CurrentMapIs(Map? map)
        {
            return StaticData.GetMap(mapid) == map;
        }

        public bool CurrentMusicIs(Music? music)
        {
            return StaticData.GetMusic(musicid) == music;
        }

        public string GetCurrentAction()
        {
            if (actionFrame == 0) return "None";
            int actionType = actionFrame / 100;
            switch (actionType)
            {
                case 1: return "Carrot Bomb";
                case 2: return "Bunny Whirl";
                case 4: return "Hammer Roll";
                case 7: return "Hammer Combo 1";
                case 8: return "Hammer Combo 2";
                case 0: return "Hammer Combo 3";
                case 9: return "Hammer Combo 4";
                case 10: return "Hammer Drill";
                case 13: return "Air Dash";
                case 14: return "Down Drill";
                case 100: return "Up Drill";
                case 110: return "Walking Hammer";
                case 120: return "Bunny Strike";
                case 150: return "Amulet";
                default: return "Unknown";
            }
        }

        public bool IsDeathSprite()
        {
            return currentSprite == 31;
        }

        public string GetCurrentSprite()
        {
            switch (currentSprite)
            {
                case 0: return "Idle";
                case 1: return "Walking";
                case 2: return "Jump";
                case 3: return "Air Jump";
                case 4: return "Falling";
                case 5: return "Slide";
                case 6: return "Carrot Bomb";
                case 7: return "Hurt";
                case 9: return "Bunny Whirl";
                case 15: return "Hammer Roll";
                case 20: return "Hammer Combo 1/4";
                case 21: return "Hammer Combo 2";
                case 10: return "Hammer Combo 3";
                case 22: return "Hammer Drill";
                case 23: return "Air Dash";
                case 24: return "Down Drill";
                case 25: return "Up Drill";
                case 26: return "Walking Hammer";
                case 27: return "Amulet";
                case 31: return "Death";
                case 32: return "Bunny Strike";
                default: return "Unknown";
            }
        }
    }
}
