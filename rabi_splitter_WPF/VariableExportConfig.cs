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
                    handle: "deaths",
                    displayName: "Deaths",
                    tracker: () => inGameState == null ? 0 : inGameState.nDeaths
                ),

                ExportVariable<int> (
                    handle: "restarts",
                    displayName: "Restarts",
                    tracker: () => inGameState == null ? 0 : inGameState.nRestarts
                ),

                ExportVariable<string> (
                    handle: "currentBoss",
                    displayName: "Current Boss Fight",
                    tracker: () => (inGameState == null || !inGameState.CurrentActivityIs(InGameActivity.BOSS_BATTLE)) ? 
                                    "None" :
                                    inGameState.currentBossFight.displayName
                ),

                ExportVariable<TimeSpan> (
                    handle: "currentBossTime",
                    displayName: "Current Boss Time",
                    tracker: () => (inGameState == null || !inGameState.CurrentActivityIs(InGameActivity.BOSS_BATTLE)) ?
                                    TimeSpan.Zero :
                                    (DateTime.Now - inGameState.currentBossStartTime)
                ),

                ExportVariable<string> (
                    handle: "lastBoss",
                    displayName: "Last Boss Fight",
                    tracker: () => inGameState?.lastBossFight == null ? "None" : inGameState.lastBossFight.displayName
                ),

                ExportVariable<TimeSpan> (
                    handle: "lastBossTime",
                    displayName: "Last Boss Time",
                    tracker: () => inGameState?.lastBossFight == null ? TimeSpan.Zero : inGameState.lastBossFightDuration
                ),

                ExportVariable<int> (
                    handle: "musicid",
                    displayName: "Music Id",
                    tracker: () => snapshot == null ? 0 : snapshot.musicid
                ),

                ExportVariable<string> (
                    handle: "music",
                    displayName: "Music",
                    tracker: () => snapshot == null ? "" : StaticData.GetMusicName(snapshot.musicid)
                ),

                ExportVariable<int> (
                    handle: "mapid",
                    displayName: "Map Id",
                    tracker: () => snapshot == null ? 0 : snapshot.mapid
                ),

                ExportVariable<string> (
                    handle: "map",
                    displayName: "Map",
                    tracker: () => snapshot == null ? "" : StaticData.GetMapName(snapshot.mapid)
                ),

                ExportVariable<string> (
                    handle: "mapTile",
                    displayName: "Map Tile",
                    tracker: () => snapshot == null ? "" : snapshot.mapTile.ToString()
                ),

                ExportVariable<int> (
                    handle: "hp",
                    displayName: "HP",
                    tracker: () => snapshot == null ? 0 : snapshot.hp
                ),

                ExportVariable<int> (
                    handle: "maxhp",
                    displayName: "Max HP",
                    tracker: () => snapshot == null ? 0 : snapshot.maxhp
                ),

                ExportVariable<float> (
                    handle: "itempercent",
                    displayName: "Item %",
                    tracker: () => snapshot == null ? 0 : snapshot.itemPercent
                ),

                ExportVariable<int> (
                    handle: "hammerXp",
                    displayName: "Hammer Exp",
                    tracker: () => snapshot == null ? 0 : snapshot.hammerXp
                ),

                ExportVariable<int> (
                    handle: "nextHammerExp",
                    displayName: "Next Hammer Level Exp",
                    tracker: () => snapshot?.nextHammer == null ? 0 : snapshot.nextHammer.Item1
                ),

                ExportVariable<string> (
                    handle: "nextHammerName",
                    displayName: "Next Hammer Level Name (Short)",
                    tracker: () => snapshot?.nextHammer == null ? "" : snapshot.nextHammer.Item2
                ),

                ExportVariable<string> (
                    handle: "nextHammerNameLong",
                    displayName: "Next Hammer Level Name (Long)",
                    tracker: () => snapshot?.nextHammer == null ? "" : snapshot.nextHammer.Item3
                ),

                ExportVariable<int> (
                    handle: "ribbonXp",
                    displayName: "Ribbon Exp",
                    tracker: () => snapshot == null ? 0 : snapshot.ribbonXp
                ),

                ExportVariable<int> (
                    handle: "nextRibbonExp",
                    displayName: "Next Ribbon Level Exp",
                    tracker: () => snapshot?.nextRibbon == null ? 0 : snapshot.nextRibbon.Item1
                ),

                ExportVariable<string> (
                    handle: "nextRibbonName",
                    displayName: "Next Ribbon Level Name (Short)",
                    tracker: () => snapshot?.nextRibbon == null ? "" : snapshot.nextRibbon.Item2
                ),

                ExportVariable<string> (
                    handle: "nextRibbonNameLong",
                    displayName: "Next Ribbon Level Name (Long)",
                    tracker: () => snapshot?.nextRibbon == null ? "" : snapshot.nextRibbon.Item3
                ),

                ExportVariable<int> (
                    handle: "carrotXp",
                    displayName: "Carrot Exp",
                    tracker: () => snapshot == null ? 0 : snapshot.carrotXp
                ),

                ExportVariable<int> (
                    handle: "nextCarrotExp",
                    displayName: "Next Carrot Level Exp",
                    tracker: () => snapshot?.nextCarrot == null ? 0 : snapshot.nextCarrot.Item1
                ),

                ExportVariable<string> (
                    handle: "nextCarrotName",
                    displayName: "Next Carrot Level Name (Short)",
                    tracker: () => snapshot?.nextCarrot == null ? "" : snapshot.nextCarrot.Item2
                ),

                ExportVariable<string> (
                    handle: "nextCarrotNameLong",
                    displayName: "Next Carrot Level Name (Long)",
                    tracker: () => snapshot?.nextCarrot == null ? "" : snapshot.nextCarrot.Item3
                ),

                ExportVariable<float> (
                    handle: "amulet",
                    displayName: "Amulet",
                    tracker: () => snapshot == null ? 0 : snapshot.amulet
                ),

                ExportVariable<int> (
                    handle: "boost",
                    displayName: "Boost",
                    tracker: () => snapshot == null ? 0 : snapshot.boost
                ),

                ExportVariable<float> (
                    handle: "mana",
                    displayName: "MP",
                    tracker: () => snapshot == null ? 0 : snapshot.mana
                ),

                ExportVariable<int> (
                    handle: "stamina",
                    displayName: "SP",
                    tracker: () => snapshot == null ? 0 : snapshot.stamina
                ),

                ExportVariable<int> (
                    handle: "healthups",
                    displayName: "No. of Health Ups",
                    tracker: () => snapshot == null ? 0 : snapshot.nHpUps
                ),

                ExportVariable<int> (
                    handle: "manaups",
                    displayName: "No. of Mana Ups",
                    tracker: () => snapshot == null ? 0 : snapshot.nManaUps
                ),

                ExportVariable<int> (
                    handle: "regenups",
                    displayName: "No. of Regen Ups",
                    tracker: () => snapshot == null ? 0 : snapshot.nRegenUps
                ),
                
                ExportVariable<int> (
                    handle: "packups",
                    displayName: "No. of Pack Ups",
                    tracker: () => snapshot == null ? 0 : snapshot.nPackUps
                ),

                ExportVariable<int> (
                    handle: "attackups",
                    displayName: "No. of Attack Ups",
                    tracker: () => snapshot == null ? 0 : snapshot.nAttackUps
                ),


                ExportVariable<float> (
                    handle: "x",
                    displayName: "x",
                    tracker: () => snapshot == null ? 0 : snapshot.px
                ),

                ExportVariable<float> (
                    handle: "y",
                    displayName: "y",
                    tracker: () => snapshot == null ? 0 : snapshot.py
                ),
                
                ExportVariable<int> (
                    handle: "playtime",
                    displayName: "Playtime",
                    tracker: () => snapshot == null ? 0 : snapshot.playtime
                ),

                ExportVariable<int> (
                    handle: "blackness",
                    displayName: "Blackness",
                    tracker: () => snapshot == null ? 0 : snapshot.blackness
                ),
            });
        }

        private ExportableVariable<T> ExportVariable<T>(string handle, string displayName, Func<T> tracker)
        {
            return new ExportableVariable<T>(handle, displayName, tracker);
        }
    }
}
