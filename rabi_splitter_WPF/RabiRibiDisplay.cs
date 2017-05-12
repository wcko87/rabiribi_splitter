using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace rabi_splitter_WPF
{
    partial class RabiRibiDisplay
    {
        private MainContext mainContext;
        private DebugContext debugContext;
        private VariableExportContext variableExportContext;
        private MainWindow mainWindow;
        
        private RabiRibiState rabiRibiState;
        private InGameState inGameState;
        private MemorySnapshot prevSnapshot;
        private MemorySnapshot snapshot;

        // Variables used for tracking frequency of memory reads.
        private static readonly DateTime UNIX_START = new DateTime(1970, 1, 1);
        private double readFps = -1;
        private long previousFrameMillisecond = -1;
        private long lastUpdateMillisecond = -1;

        // internal frame counter.
        private int memoryReadCount;

        public RabiRibiDisplay(MainContext mainContext, DebugContext debugContext, VariableExportContext variableExportContext, MainWindow mainWindow)
        {
            this.rabiRibiState = new RabiRibiState();
            this.mainContext = mainContext;
            this.debugContext = debugContext;
            this.variableExportContext = variableExportContext;
            this.mainWindow = mainWindow;
            this.memoryReadCount = 0;
            StartNewGame();
            ConfigureVariableExports();
        }

        public void ReadMemory(Process process)
        {
            ++memoryReadCount;
            var memoryHelper = new MemoryHelper(process);

            // Snapshot Game Memory
            snapshot = new MemorySnapshot(memoryHelper, mainContext.veridx);

            Update();
            UpdateDebugArea(memoryHelper);
            UpdateEntityData(memoryHelper);
            UpdateFps();
            UpdateVariableExport();

            if (snapshot.musicid >= 0) rabiRibiState.lastValidMusicId = snapshot.musicid;
            prevSnapshot = snapshot;

            memoryHelper.Dispose();
        }

        private void UpdateVariableExport()
        {
            long currentFrameMillisecond = (long)(DateTime.Now - UNIX_START).TotalMilliseconds;
            var diff = currentFrameMillisecond - lastUpdateMillisecond;
            if (diff >= 1000)
            {
                if (diff >= 2000) lastUpdateMillisecond = currentFrameMillisecond;
                else lastUpdateMillisecond += 1000;
                variableExportContext.UpdateVariables(true);
            }
            else
            {
                // Don't update files.
                variableExportContext.UpdateVariables(false);
            }
        }
        
        private void UpdateFps()
        {
            long currentFrameMillisecond = (long)(DateTime.Now - UNIX_START).TotalMilliseconds;
            if (previousFrameMillisecond != -1)
            {
                double newFps = 1000.0 / (currentFrameMillisecond - previousFrameMillisecond);
                if (readFps < 0) readFps = newFps;
                else readFps = 0.9 * readFps + 0.1 * newFps;

                mainContext.Text19 = $"Reads Per Second:\n{readFps:.0.00}";
            }
            previousFrameMillisecond = currentFrameMillisecond;
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
                    var bossFight = BossFightIdentifier.IdentifyBossFight(snapshot);
                    DebugLog($"BOSS FIGHT: {bossFight.displayName}");
                    DebugLog($"Fighting Bosses: {string.Join(", ", snapshot.bossList.Select(boss => StaticData.GetBossName(boss.id)))}");

                    inGameState.StartBossFight(bossFight);
                }
                else // snapshot.minimapPosition == 0
                {
                    if (reloading)
                    {
                        inGameState.StopBossFight();
                    }
                    else
                    {
                        inGameState.FinishBossFight();
                    }
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

            mainContext.Text15 = $"Map Tile: ({snapshot.mapTile.x}, {snapshot.mapTile.y})";

            {
                if (inGameState.CurrentActivityIs(InGameActivity.BOSS_BATTLE))
                {
                    var time = DateTime.Now - inGameState.currentBossStartTime;
                    mainContext.Text16 = $"Boss: {inGameState.currentBossFight.displayName}\n" +
                                         $"Time: {time:mm\\:ss\\.ff}";
                }
                else
                {
                    mainContext.Text16 = "Not in boss fight";
                }

                if (inGameState.lastBossFight != null)
                {
                    mainContext.Text17 = $"Last Boss: {inGameState.lastBossFight.displayName}\n" +
                                         $"Time: {inGameState.lastBossFightDuration:mm\\:ss\\.ff}";
                }
                else
                {
                    mainContext.Text17 = "Last Boss: None";
                }
            }

            {
                string bosstext = "Boss Fight: " + (inGameState.currentActivity == InGameActivity.BOSS_BATTLE) + "\n";
                bosstext += "Bosses: " + snapshot.bossList.Count + "\n";
                foreach (var boss in snapshot.bossList)
                {
                    bosstext += "[" + boss.entityArrayIndex + "] " + StaticData.GetBossName(boss.id) + ": " + boss.hp + "/" + boss.maxHp + "\n";
                }

                mainContext.Text20 = bosstext;
            }
        }

        private void UpdateTextFile()
        {
            //return;
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
        
        private void UpdateEntityData(MemoryHelper memoryHelper)
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
                    int value_int = memoryHelper.GetMemoryValue<int>(baseArrayPtr + i, false);
                    float value_float = memoryHelper.GetMemoryValue<float>(baseArrayPtr + i, false);
                    entityStatsList[index].IntVal = value_int;
                    entityStatsList[index].FloatVal = value_float;
                }
            }
        }

        private void UpdateDebugArea(MemoryHelper memoryHelper)
        {
            int ptr = snapshot.entityArrayPtr;
            //                    List<int> bosses = new List<int>();
            //                    List<int> HPS = new List<int>();
            //                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => debugContext.BossList.Clear()));
            //                    ptr += StaticData.EnenyEntitySize[mainContext.veridx] * 3;
            for (var i = 0; i < 50; i++)
            {
                debugContext.BossList[i].BossID = memoryHelper.GetMemoryValue<int>(
                    ptr + StaticData.EnenyEnitiyIDOffset[mainContext.veridx], false);
                debugContext.BossList[i].BossHP = memoryHelper.GetMemoryValue<int>(
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
