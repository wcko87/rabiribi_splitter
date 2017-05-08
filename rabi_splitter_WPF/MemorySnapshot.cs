using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace rabi_splitter_WPF
{
    public struct MapTileCoordinate
    {
        public readonly int x;
        public readonly int y;

        public MapTileCoordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static MapTileCoordinate FromWorldPosition(int mapid, float px, float py)
        {
            // Note: a game-tile is 64x64
            // A map-tile is 1280x720. (20 x 11.25 game tiles)
            int x = (int)(px / 1280) + mapid * 25;
            int y = (int)(py / 720);
            
            return new MapTileCoordinate(x, y);
        }

        #region Equals, Hashcode
        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            var other = (MapTileCoordinate)obj;
            return x == other.x && y == other.y;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return x.GetHashCode() * 31 + y.GetHashCode();
        }
        #endregion
    }

    public struct BossStats
    {
        public int entityArrayIndex;
        public int id;
        public Boss type;
        public int hp;
        public int maxHp;
    }

    public class MemorySnapshot
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

        public readonly float amulet;
        public readonly int boost;
        public readonly float mana;
        public readonly int stamina;

        public readonly float px;
        public readonly float py;
        public readonly MapTileCoordinate mapTile;

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

        public readonly int minimapPosition;

        public MemorySnapshot(MemoryHelper memoryHelper, int veridx)
        {
            t_playtime = memoryHelper.GetMemoryValue<int>(StaticData.IGTAddr[veridx]);
            playtime = memoryHelper.GetMemoryValue<int>(StaticData.PlaytimeAddr[veridx]);
            blackness = memoryHelper.GetMemoryValue<int>(StaticData.BlacknessAddr[veridx]);

            mapid = memoryHelper.GetMemoryValue<int>(StaticData.MapAddress[veridx]);
            musicid = memoryHelper.GetMemoryValue<int>(StaticData.MusicAddr[veridx]);
            money = memoryHelper.GetMemoryValue<int>(StaticData.MoneyAddress[veridx]);

            carrotXp = memoryHelper.GetMemoryValue<int>(0xD654BC);
            hammerXp = memoryHelper.GetMemoryValue<int>(0xD654B4);
            ribbonXp = memoryHelper.GetMemoryValue<int>(0xD654B8);
            itemPercent = memoryHelper.GetMemoryValue<float>(0xA730E8);

            minimapPosition = memoryHelper.GetMemoryValue<int>(0xA72E08);

            nAttackUps = countItems(memoryHelper, 0xD6352C, 0xD63628);
            nHpUps = countItems(memoryHelper, 0xD6342C, 0xD63528);
            nManaUps = countItems(memoryHelper, 0xD6362C, 0xD63728);
            nPackUps = countItems(memoryHelper, 0xD6382C, 0xD63928);
            nRegenUps = countItems(memoryHelper, 0xD6372C, 0xD63828);

            entityArrayPtr = memoryHelper.GetMemoryValue<int>(StaticData.EnenyPtrAddr[veridx]);

            hp = memoryHelper.GetMemoryValue<int>(entityArrayPtr + 0x4D8, false);
            maxhp = memoryHelper.GetMemoryValue<int>(entityArrayPtr + 0x4E8, false);

            currentSprite = memoryHelper.GetMemoryValue<int>(entityArrayPtr + 0x654, false);
            actionFrame = memoryHelper.GetMemoryValue<int>(entityArrayPtr + 0x660, false);

            amulet = memoryHelper.GetMemoryValue<float>(entityArrayPtr + 0x52C, false);
            boost = memoryHelper.GetMemoryValue<int>(entityArrayPtr + 0x5DC, false);
            mana = memoryHelper.GetMemoryValue<float>(entityArrayPtr + 0x6B8, false);
            stamina = memoryHelper.GetMemoryValue<int>(entityArrayPtr + 0x5B4, false);

            px = memoryHelper.GetMemoryValue<float>(entityArrayPtr + 0xC, false);
            py = memoryHelper.GetMemoryValue<float>(entityArrayPtr + 0x10, false);
            mapTile = MapTileCoordinate.FromWorldPosition(mapid, px, py);
            
            // Read Entity Array and Search for boss data
            bossList = new List<BossStats>();
            nActiveEntities = 0;
            entityArraySize = 4;
            int entitySize = StaticData.EnenyEntitySize[veridx];
            int currArrayPtr = entityArrayPtr + entitySize * 4;
            for (int i=0; i<500; ++i) {
                // (Hard limit of reading 500 entries)
                int entityId = memoryHelper.GetMemoryValue<int>(
                    currArrayPtr + StaticData.EnenyEnitiyIDOffset[veridx], false);
                int entityMaxHp = memoryHelper.GetMemoryValue<int>(
                    currArrayPtr + StaticData.EnenyEnitiyMaxHPOffset[veridx], false);

                if (entityId == 0 && entityMaxHp == 0) break;

                int activeFlag = memoryHelper.GetMemoryValue<int>(
                    currArrayPtr + StaticData.EnenyEnitiyIsActiveOffset[veridx], false);
                int animationState = memoryHelper.GetMemoryValue<int>(
                    currArrayPtr + StaticData.EnenyEnitiyAnimationOffset[veridx], false);

                bool isAlive = activeFlag == 1 && animationState >= 0;
                
                if (isAlive && StaticData.IsBoss(entityId))
                {
                    BossStats boss;
                    boss.entityArrayIndex = entityArraySize;
                    boss.id = entityId;
                    boss.hp = memoryHelper.GetMemoryValue<int>(currArrayPtr + StaticData.EnenyEnitiyHPOffset[veridx], false);
                    boss.type = StaticData.GetBoss(entityId).Value;
                    boss.maxHp = entityMaxHp;

                    bossList.Add(boss);
                }

                currArrayPtr += entitySize;

                if (isAlive) ++nActiveEntities;
                ++entityArraySize;
            }
        }

        private int countItems(MemoryHelper memoryHelper, int addrFirst, int addrLast)
        {
            int count = 0;
            for (int addr = addrFirst; addr <= addrLast; ++addr)
            {
                count += memoryHelper.GetMemoryValue<int>(addr) == 1 ? 1 : 0;
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
