using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rabi_splitter_WPF
{
    partial class RabiRibiDisplay
    {
        void ConfigureVariableExports()
        {
            variableExportContext.DefineVariableExports(new ExportableVariable[] {
                ExportVariable<int> (
                    handle: "playtime",
                    displayName: "Playtime",
                    tracker: () => snapshot.playtime
                ),

                ExportVariable<int> (
                    handle: "blackness",
                    displayName: "Blackness",
                    tracker: () => snapshot.blackness
                ),

                ExportVariable<int> (
                    handle: "mapid",
                    displayName: "Map Id",
                    tracker: () => snapshot.mapid
                ),

                ExportVariable<string> (
                    handle: "map",
                    displayName: "Map",
                    tracker: () => StaticData.GetMapName(snapshot.mapid)
                ),

                ExportVariable<int> (
                    handle: "musicid",
                    displayName: "Music Id",
                    tracker: () => snapshot.musicid
                ),

                ExportVariable<string> (
                    handle: "music",
                    displayName: "Music",
                    tracker: () => StaticData.GetMusicName(snapshot.musicid)
                ),

                ExportVariable<int> (
                    handle: "hp",
                    displayName: "HP",
                    tracker: () => snapshot.hp
                ),

                ExportVariable<float> (
                    handle: "amulet",
                    displayName: "Amulet",
                    tracker: () => snapshot.amulet
                ),

                ExportVariable<int> (
                    handle: "boost",
                    displayName: "Boost",
                    tracker: () => snapshot.boost
                ),

                ExportVariable<float> (
                    handle: "mana",
                    displayName: "MP",
                    tracker: () => snapshot.mana
                ),

                ExportVariable<int> (
                    handle: "stamina",
                    displayName: "SP",
                    tracker: () => snapshot.stamina
                ),

                ExportVariable<float> (
                    handle: "x",
                    displayName: "x",
                    tracker: () => snapshot.px
                ),

                ExportVariable<float> (
                    handle: "y",
                    displayName: "y",
                    tracker: () => snapshot.py
                ),

                ExportVariable<MapTileCoordinate> (
                    handle: "mapTile",
                    displayName: "Map Tile",
                    tracker: () => snapshot.mapTile
                ),

                ExportVariable<int> (
                    handle: "nDeaths",
                    displayName: "Deaths",
                    tracker: () => inGameState.nDeaths
                ),

                ExportVariable<int> (
                    handle: "nRestarts",
                    displayName: "Restarts",
                    tracker: () => inGameState.nRestarts
                ),
            });
        }

        private ExportableVariable<T> ExportVariable<T>(string handle, string displayName, Func<T> tracker)
        {
            return new ExportableVariable<T>(handle, displayName, tracker);
        }
    }
}
