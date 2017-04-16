using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rabi_splitter_WPF
{
    public enum Boss
    {
        Cocoa,
        Rumi,
        Ashuri,
        Rita,
        Ribbon,
        Cocoa2,
        Cicini,
        Saya,
        Syaro,
        Pandora,
        Nieve,
        Nixie,
        Aruraune,
        Seana,
        Lilith,
        Vanilla,
        Chocolate,
        IllusionAlius,
        PinkKotri,
        Noah1,
        Irisu,
        Miriam,
        Miru,
        Noah3,
        KekeBunny,
    }

    public enum Map
    {
        SouthernWoodland,
        WesternCoast,
        IslandCore,
        NorthernTundra,
        EasternHighlands,
        RabiRabiTown,
        Plurkwood,
        SubterraneanArea,
        WarpDestination,
        SystemInterior,
    }

    public enum Music
    {
        NO_MUSIC,
        ADVENTURE_STARTS_HERE,
        SPECTRAL_CAVE,
        FORGOTTEN_CAVE,
        UNDERWATER_AMBIENT,
        LIBRARY_AMBIENT,
        FORGOTTEN_CAVE_II,
        STARTING_FOREST_NIGHT,
        BOUNCE_BOUNCE,
        RABI_RABI_BEACH,
        PANDORAS_PALACE,
        RABI_RABI_RAVINE,
        HOME_SWEET_HOME,
        RABI_RABI_PARK,
        INSIDE_UPRPRC,
        SKY_ISLAND_TOWN,
        WINTER_WONDERLAND,
        CYBERSPACE_EXE,
        EVERNIGHT_PEAK,
        EXOTIC_LABORATORY,
        GOLDEN_RIVERBANK,
        FLOATING_GRAVEYARD,
        SYSTEM_INTERIOR_II,
        AURORA_PALACE,
        SPEICHER_GALERIE,
        DEEP_UNDER_THE_SEA,
        SKY_HIGH_BRIDGE,
        WARP_DESTINATION,
        VOLCANIC_CANERNS,
        PLURKWOOD,
        ANOTHER_D,
        ICY_SUMMIT,
        PREPARE_EVENT,
        MIDBOSS_BATTLE,
        MIDSTREAM_JAM,
        MIRIAMS_SHOP,
        BUNNY_PANIC,
        THE_TRUTH_NEVER_SPOKEN,
        BRAWL_BREAKS_VER_2,
        BRAWL_BREAKS,
        SANDBAG_MINI_GAME,
        STAFF_ROLL,
        RFN_III,
        NO_REMORSE,
        GET_ON_WITH_IT,
        THEME_OF_RABI_RIBI_8BIT,
        THEME_OF_RABI_RIBI,
        FULL_ON_COMBAT,
        HI_TECH_DUEL,
        UNFAMILIAR_PLACE,
        UNFAMILIAR_PLACE_AGAIN,
        KITTY_ATTACK,
        M_R_,
        MAIN_MENU,
        SUDDEN_DEATH,
        RABI_RABI_RAVINE_VER_2,
        WASTE,
        ARTBOOK_INTRO,
        RABI_RIBI_PIANO_TITLE,
        MISCHIEVOUS_MASQUERADE,
    }

    // A set of (id, enum, string)
    public class IdEnumAssociation<EnumType> : List<Tuple<int, EnumType, string>>
    {
        public void Add(int id, EnumType value, string name)
        {
            Add(new Tuple<int, EnumType, string>(id, value, name));
        }
    }

    // A set of (enum, string)
    public class IndexEnumAssociation<EnumType> : List<Tuple<EnumType, string>>
    {
        public void Add(EnumType value, string name)
        {
            Add(new Tuple<EnumType, string>(value, name));
        }
    }

    public static partial class StaticData {

        public static readonly Dictionary<int, Boss> _getBoss;
        public static readonly Dictionary<int, string> _getBossName;
        public static readonly Dictionary<Boss, string> _getBossFromType;

        public static readonly Map[] _getMap;
        public static readonly string[] _getMapName;
        public static readonly Dictionary<Map, string> _getMapFromType;

        public static readonly Music[] _getMusic;
        public static readonly string[] _getMusicName;
        public static readonly Dictionary<Music, string> _getMusicFromType;

        static StaticData()
        {
            _getBoss = BossList.ToDictionary(t => t.Item1, t => t.Item2);
            _getBossName = BossList.ToDictionary(t => t.Item1, t => t.Item3);
            _getBossFromType = BossList.ToDictionary(t => t.Item2, t => t.Item3);

            _getMap = MapList.Select(t => t.Item1).ToArray();
            _getMapName = MapList.Select(t => t.Item2).ToArray();
            _getMapFromType = MapList.ToDictionary(t => t.Item1, t => t.Item2);

            _getMusic = MusicList.Select(t => t.Item1).ToArray();
            _getMusicName = MusicList.Select(t => t.Item2).ToArray();
            _getMusicFromType = MusicList.ToDictionary(t => t.Item1, t => t.Item2);
        }

        public static Boss? GetBoss(int id) {
            Boss value;
            if (_getBoss.TryGetValue(id, out value)) return value;
            return null;
        }

        public static string GetBossName(int id) {
            string value;
            if (_getBossName.TryGetValue(id, out value)) return value;
            return "";
        }

        public static string GetBossName(Boss? boss) {
            string value;
            if (boss.HasValue && _getBossFromType.TryGetValue(boss.Value, out value)) return value;
            return "";
        }

        public static Map? GetMap(int id) {
            if (0 <= id && id < _getMap.Length) return _getMap[id];
            return null;
        }

        public static string GetMapName(int id) {
            if (0 <= id && id < _getMapName.Length) return _getMapName[id];
            return "Unknown ID " + id;
        }

        public static string GetMapName(Map? map) {
            string value;
            if (map.HasValue && _getMapFromType.TryGetValue(map.Value, out value)) return value;
            return "";
        }

        public static Music? GetMusic(int id) {
            if (0 <= id && id < _getMusic.Length) return _getMusic[id];
            return null;
        }

        public static string GetMusicName(int id) {
            if (0 <= id && id < _getMusicName.Length) return _getMusicName[id];
            return "Unknown ID " + id;
        }

        public static string GetMusicName(Music? music) {
            string value;
            if (music.HasValue && _getMusicFromType.TryGetValue(music.Value, out value)) return value;
            return "";
        }
    }
}
