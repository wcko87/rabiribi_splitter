using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace rabi_splitter_WPF
{
    class RabiRibiDisplay
    {
        private MainContext mainContext;
        private DebugContext debugContext;
        private MainWindow mainWindow;
        
        private RabiRibiState rabiRibiState;
        private InGameState inGameState;
        private MemorySnapshot prevSnapshot;
        private MemorySnapshot snapshot;

        // internal frame counter.
        private int memoryReadCount;

        public RabiRibiDisplay(MainContext mainContext, DebugContext debugContext, MainWindow mainWindow)
        {
            this.rabiRibiState = new RabiRibiState();
            this.mainContext = mainContext;
            this.debugContext = debugContext;
            this.mainWindow = mainWindow;
            this.memoryReadCount = 0;
            StartNewGame();
        }

        public void ReadMemory(Process process)
        {
            ++memoryReadCount;

            // Snapshot Game Memory
            snapshot = new MemorySnapshot(process, mainContext.veridx);

            Update();
            UpdateDebugArea(process);
            UpdateEntityData(process);

            if (snapshot.musicid >= 0) rabiRibiState.lastValidMusicId = snapshot.musicid;
            prevSnapshot = snapshot;
        }

        private void Update()
        {
            #region Game State Machine

            if (inGameState.CurrentActivityIs(InGameActivity.STARTING)) {
                // Detect start game
                if (0 < snapshot.playtime && snapshot.playtime < 200 ||
                    (prevSnapshot != null && prevSnapshot.playtime < snapshot.playtime && snapshot.playtime < prevSnapshot.playtime + 200))
                {
                    inGameState.currentActivity = InGameActivity.WALKING;
                    DebugLog("IGT start?");
                }
            } else {
                if (MusicChanged())
                {
                    if (StaticData.IsBossMusic(snapshot.musicid))
                    {
                        inGameState.currentActivity = InGameActivity.BOSS_BATTLE;
                    }
                    else
                    {
                        inGameState.currentActivity = InGameActivity.WALKING;
                    }
                }
            }

            #endregion

            #region Detect Music change

            if (MusicChanged() && !ValidMusicChanged())
            {
                DebugLog($"Invalid Music Change: {StaticData.GetMusicName(prevSnapshot.musicid)} -> {StaticData.GetMusicName(snapshot.musicid)}");
            }

            if (ValidMusicChanged())
            {
                DebugLog($"Valid Music Change: {StaticData.GetMusicName(rabiRibiState.lastValidMusicId)} -> {StaticData.GetMusicName(snapshot.musicid)}");
            }

            #endregion

            #region Detect Minimap Change

            if (prevSnapshot != null && (prevSnapshot.minimapPosition != snapshot.minimapPosition))
            {
                DebugLog($"Minimap Shift! {prevSnapshot.minimapPosition} -> {snapshot.minimapPosition}");
                if (snapshot.minimapPosition == 1)
                {
                    DebugLog($"Fighting Boss: {string.Join(", ", snapshot.bossList.Select(boss => StaticData.GetBossName(boss.id)))}");
                }
            }

            #endregion

            #region Detect Boss Change

            if (prevSnapshot != null)
            {
                var currBosses = new HashSet<int>(snapshot.bossList.Select(bossStats => bossStats.id));
                var prevBosses = new HashSet<int>(prevSnapshot.bossList.Select(bossStats => bossStats.id));

                foreach (var enteringBoss in currBosses.Except(prevBosses))
                {
                    DebugLog($"Boss Enters: {StaticData.GetBossName(enteringBoss)}");
                }

                foreach (var leavingBoss in prevBosses.Except(currBosses))
                {
                    DebugLog($"Boss Leaves: {StaticData.GetBossName(leavingBoss)}");
                }
            }

            #endregion

            #region Detect Reload

            bool reloading = snapshot.playtime == 0 || ((prevSnapshot != null) && (snapshot.playtime < prevSnapshot.playtime));
            if (inGameState.IsGameStarted() && snapshot.playtime > 0)
            {
                if (snapshot.playtime < inGameState.lastNonZeroPlayTime)
                {
                    if (InGame())
                    {
                        inGameState.nRestarts++;
                        UpdateTextFile();
                    }
                    DebugLog("Reload Game! " + snapshot.playtime + " <- " + inGameState.lastNonZeroPlayTime);
                }
                inGameState.lastNonZeroPlayTime = snapshot.playtime;
            }

            #endregion

            #region Detect Death
            
            if (prevSnapshot != null)
            {
                if (snapshot.hp == 0 && prevSnapshot.hp > 0)
                {
                    if (InGame())
                    {
                        inGameState.nDeaths++;
                        UpdateTextFile();
                    }
                    DebugLog("Death!");
                }
            }

            if (prevSnapshot != null)
            {
                if (snapshot.IsDeathSprite() && !prevSnapshot.IsDeathSprite())
                {
                    if (InGame())
                    {
                        inGameState.nDeathsAlt++;
                    }
                    DebugLog("Death (Alt)!");
                }
            }

            #endregion

            #region Detect Start Game

            if (prevSnapshot != null && (snapshot.CurrentMusicIs(Music.MAIN_MENU) || snapshot.CurrentMusicIs(Music.ARTBOOK_INTRO))
                && prevSnapshot.blackness == 0 && snapshot.blackness >= 100000)
            {
                // Sudden increase by 100000
                DebugLog("Start Game!");
                StartNewGame();
            }

            #endregion
           
            if (prevSnapshot == null || prevSnapshot.mapid != snapshot.mapid)
            {
                DebugLog("newmap: " + snapshot.mapid + ":" + StaticData.GetMapName(snapshot.mapid));
            }

            mainContext.Text1 = "Music: " + StaticData.GetMusicName(snapshot.musicid);
            mainContext.Text2 = "Map: " + StaticData.GetMapName(snapshot.mapid);
            mainContext.Text3 = inGameState == null ? "" : ("Deaths: " + inGameState.nDeaths// + " [" + gameState.nDeathsAlt + "]"
                                                 + "\n" + "Resets: " + inGameState.nRestarts);// + " [" + gameState.nRestartsAlt + "]");

            mainContext.Text4 = "HP: " + snapshot.hp + " / " + snapshot.maxhp;
            mainContext.Text5 = "Amulet: " + snapshot.amulet + "\n" + "Boost: " + snapshot.boost;
            mainContext.Text6 = "MP: " + snapshot.mana + "\n" + "SP: " + snapshot.stamina;

            var nextHammer = StaticData.GetNextHammerLevel(snapshot.hammerXp);
            var nextRibbon = StaticData.GetNextRibbonLevel(snapshot.ribbonXp);
            var nextCarrot = StaticData.GetNextCarrotLevel(snapshot.carrotXp);
            mainContext.Text7 = "Hammer: " + snapshot.hammerXp + (nextHammer == null ? "" : ("/" + nextHammer.Item1 + "\n" + "NEXT: " + nextHammer.Item2));
            mainContext.Text8 = "Ribbon: " + snapshot.ribbonXp + (nextRibbon == null ? "" : ("/" + nextRibbon.Item1 + "\n" + "NEXT: " + nextRibbon.Item2));
            mainContext.Text9 = "Carrot: " + snapshot.carrotXp + (nextCarrot == null ? "" : ("/" + nextCarrot.Item1 + "\n" + "NEXT: " + nextCarrot.Item2));

            mainContext.Text10 = "x: " + snapshot.px + "\n" + "y: " + snapshot.py;
            mainContext.Text11 = "[A/H/M/P/R] ups:\n" + snapshot.nAttackUps + "/" + snapshot.nHpUps + "/" + snapshot.nManaUps + "/" + snapshot.nPackUps + "/" + snapshot.nRegenUps;


            mainContext.Text12 = "Entities: " + snapshot.entityArraySize + "\n" + "Active: " + snapshot.nActiveEntities;

            mainContext.Text13 = "Sprite: " + snapshot.GetCurrentSprite() + "\n" + "Action: " + snapshot.GetCurrentAction();

            mainContext.Text14 = $"PLAYTIME: {snapshot.playtime}";

            {
                string bosstext = "Boss Fight: " + (inGameState.currentActivity == InGameActivity.BOSS_BATTLE) + "\n";
                bosstext += "Bosses: " + snapshot.bossList.Count + "\n";
                foreach (var boss in snapshot.bossList)
                {
                    bosstext += "[" + boss.entityArrayIndex + "] " + StaticData.GetBossName(boss.id) + ": " + boss.hp + "/" + boss.maxHp + "\n";
                }

                mainContext.Text16 = bosstext;
            }
        }

        private void UpdateTextFile()
        {
            string text = $"Deaths: {inGameState.nDeaths}\nResets: {inGameState.nRestarts}";
            System.IO.StreamWriter file = new System.IO.StreamWriter("deaths_restarts.txt");
            file.WriteLine(text);
            file.Close();
        }

        private void StartNewGame()
        {
            inGameState = new InGameState();
            rabiRibiState.gameStatus = GameStatus.INGAME;
        }

        private void ReturnToMenu()
        {
            rabiRibiState.gameStatus = GameStatus.MENU;
            inGameState = null;
        }

        private bool InGame()
        {
            return rabiRibiState.gameStatus == GameStatus.INGAME;
        }

        private bool MusicChanged()
        {
            return prevSnapshot != null && prevSnapshot.musicid != snapshot.musicid;
        }

        private bool ValidMusicChanged()
        {
            return rabiRibiState.lastValidMusicId >= 0 && snapshot.musicid >= 0 && rabiRibiState.lastValidMusicId != snapshot.musicid;
        }

        private bool MusicChangedTo(Music music)
        {
            return MusicChanged() && snapshot.CurrentMusicIs(music);
        }
        
        private void UpdateEntityData(Process process)
        {
            // Read entire entity data for specific entity
            {
                var entityStatsList = debugContext.EntityStatsListData;

                int entitySize = StaticData.EnenyEntitySize[mainContext.veridx];
                int baseArrayPtr = snapshot.entityArrayPtr + entitySize * debugContext.EntityAnalysisIndex;
                int[] entitydataint = new int[entitySize / 4];
                float[] entitydatafloat = new float[entitySize / 4];

                debugContext.targetEntityListSize = entitySize / 4;
                int length = Math.Min(entitySize, entityStatsList.Count * 4);
                for (int i = 0; i < length; i += 4)
                {
                    int index = i / 4;
                    int value_int = MemoryHelper.GetMemoryValue<int>(process, baseArrayPtr + i, false);
                    float value_float = MemoryHelper.GetMemoryValue<float>(process, baseArrayPtr + i, false);
                    entityStatsList[index].IntVal = value_int;
                    entityStatsList[index].FloatVal = value_float;
                }
            }
        }

        private void UpdateDebugArea(Process process)
        {
            int ptr = snapshot.entityArrayPtr;
            //                    List<int> bosses = new List<int>();
            //                    List<int> HPS = new List<int>();
            //                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => debugContext.BossList.Clear()));
            //                    ptr += StaticData.EnenyEntitySize[mainContext.veridx] * 3;
            for (var i = 0; i < 50; i++)
            {
                debugContext.BossList[i].BossID = MemoryHelper.GetMemoryValue<int>(process,
                    ptr + StaticData.EnenyEnitiyIDOffset[mainContext.veridx], false);
                debugContext.BossList[i].BossHP = MemoryHelper.GetMemoryValue<int>(process,
                    ptr + StaticData.EnenyEnitiyHPOffset[mainContext.veridx], false);
                ptr += StaticData.EnenyEntitySize[mainContext.veridx];
            }

            debugContext.BossEvent = inGameState.currentActivity == InGameActivity.BOSS_BATTLE;
        }

        private void DebugLog(string log)
        {
            this.debugContext.Log($"[ {memoryReadCount:D8}] {log}");
        }

        private void sendsplit()
        {
            mainWindow.SendMessage("split\r\n");
        }

        private void sendreset()
        {
            mainWindow.SendMessage("reset\r\n");
        }

        private void sendstarttimer()
        {
            mainWindow.SendMessage("starttimer\r\n");
        }

        private void sendigt(float time)
        {
            mainWindow.SendMessage($"setgametime {time}\r\n");
        }

    }
}
