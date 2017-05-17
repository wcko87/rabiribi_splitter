using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace rabi_splitter_WPF
{
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

        public readonly int entityArrayPtr;

        public readonly float amulet;
        public readonly int boost;
        public readonly float mana;
        public readonly int stamina;

        public readonly float px;
        public readonly float py;
        
        public MemorySnapshot(Process process, int veridx)
        {
            t_playtime = MemoryHelper.GetMemoryValue<int>(process, StaticData.IGTAddr[veridx]);
            playtime = MemoryHelper.GetMemoryValue<int>(process, StaticData.PlaytimeAddr[veridx]);
            blackness = MemoryHelper.GetMemoryValue<int>(process, StaticData.BlacknessAddr[veridx]);

            mapid = MemoryHelper.GetMemoryValue<int>(process, StaticData.MapAddress[veridx]);
            musicid = MemoryHelper.GetMemoryValue<int>(process, StaticData.MusicAddr[veridx]);
            money = MemoryHelper.GetMemoryValue<int>(process, StaticData.MoneyAddress[veridx]);

            entityArrayPtr = MemoryHelper.GetMemoryValue<int>(process, StaticData.EnenyPtrAddr[veridx]);

            hp = MemoryHelper.GetMemoryValue<int>(process, entityArrayPtr + 0x4D8, false);
            maxhp = MemoryHelper.GetMemoryValue<int>(process, entityArrayPtr + 0x4E8, false);

            amulet = MemoryHelper.GetMemoryValue<float>(process, entityArrayPtr + 0x52C, false);
            boost = MemoryHelper.GetMemoryValue<int>(process, entityArrayPtr + 0x5DC, false);
            mana = MemoryHelper.GetMemoryValue<float>(process, entityArrayPtr + 0x6B8, false);
            stamina = MemoryHelper.GetMemoryValue<int>(process, entityArrayPtr + 0x5B4, false);

            px = MemoryHelper.GetMemoryValue<float>(process, entityArrayPtr + 0xC, false);
            py = MemoryHelper.GetMemoryValue<float>(process, entityArrayPtr + 0x10, false);
        }
        
        public bool CurrentMapIs(Map? map)
        {
            return StaticData.GetMap(mapid) == map;
        }

        public bool CurrentMusicIs(Music? music)
        {
            return StaticData.GetMusic(musicid) == music;
        }
    }
}
