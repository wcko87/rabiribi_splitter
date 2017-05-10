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
            ExportableVariable.DefineVariableExports(new ExportableVariable[] {
                ExportVariable<int> (
                    displayName: "Playtime",
                    tracker: () => snapshot.playtime
                ),

                ExportVariable<int> (
                    displayName: "Blackness",
                    tracker: () => snapshot.blackness
                ),

                ExportVariable<int> (
                    displayName: "Map Id",
                    tracker: () => snapshot.mapid
                ),

                ExportVariable<string> (
                    displayName: "Map",
                    tracker: () => StaticData.GetMapName(snapshot.mapid)
                ),

                ExportVariable<int> (
                    displayName: "Music Id",
                    tracker: () => snapshot.musicid
                ),

                ExportVariable<string> (
                    displayName: "Music",
                    tracker: () => StaticData.GetMusicName(snapshot.musicid)
                ),

                ExportVariable<int> (
                    displayName: "HP",
                    tracker: () => snapshot.hp
                ),

                ExportVariable<float> (
                    displayName: "Amulet",
                    tracker: () => snapshot.amulet
                ),

                ExportVariable<int> (
                    displayName: "Boost",
                    tracker: () => snapshot.boost
                ),

                ExportVariable<float> (
                    displayName: "MP",
                    tracker: () => snapshot.mana
                ),

                ExportVariable<int> (
                    displayName: "SP",
                    tracker: () => snapshot.stamina
                ),

                ExportVariable<float> (
                    displayName: "x",
                    tracker: () => snapshot.px
                ),

                ExportVariable<float> (
                    displayName: "y",
                    tracker: () => snapshot.py
                ),

                ExportVariable<MapTileCoordinate> (
                    displayName: "Map Tile",
                    tracker: () => snapshot.mapTile
                ),

                ExportVariable<int> (
                    displayName: "Deaths",
                    tracker: () => inGameState.nDeaths
                ),

                ExportVariable<int> (
                    displayName: "Restarts",
                    tracker: () => inGameState.nRestarts
                ),
            });


            variableExportContext.NotifyExportableVariableUpdate();
        }

        private ExportableVariable<T> ExportVariable<T>(string displayName, Func<T> tracker)
        {
            return new ExportableVariable<T>(displayName, tracker);
        }
    }
}
