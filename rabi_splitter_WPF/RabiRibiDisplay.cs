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

        public int previousBlackness = -1;
        public int lastmoney;
        public int lastmapid;
        public int lastmusicid;
        public int lastplaytime = 0;
        public bool bossbattle;
        public List<int> lastbosslist;
        public int lastnoah3hp;
        public int lastTM;
        public DateTime LastTMAddTime;
        private bool _noah1Reload;


        public RabiRibiDisplay(MainContext mainContext, DebugContext debugContext, MainWindow mainWindow)
        {
            this.mainContext = mainContext;
            this.debugContext = debugContext;
            this.mainWindow = mainWindow;
        }

        public void ReadMemory(Process process)
        {
            #region read igt

            int igt = MemoryHelper.GetMemoryValue<int>(process, StaticData.IGTAddr[mainContext.veridx]);
            if (igt > 0 && mainContext.Igt)
            {
                sendigt((float)igt / 60);
            }

            #endregion

            #region Detect Reload

            bool reloaded = false;
            {
                int playtime = MemoryHelper.GetMemoryValue<int>(process, StaticData.PlaytimeAddr[mainContext.veridx]);
                reloaded = playtime != 0 && playtime < lastplaytime;
                if (reloaded) DebugLog("Reload Game!");
                lastplaytime = playtime;
            }

            #endregion

            #region Detect Start Game

            {
                int blackness = MemoryHelper.GetMemoryValue<int>(process, StaticData.BlacknessAddr[mainContext.veridx]);
                if (previousBlackness == 0 && blackness >= 100000)
                {
                    // Sudden increase by 100000
                    // Have to be careful, though. I don't know whether anything else causes blackness to increase by 100000
                    DebugLog("Start Game!");
                }
                previousBlackness = blackness;
            }

            #endregion

            #region CheckMoney
            
            {
                var newmoney = MemoryHelper.GetMemoryValue<int>(process, StaticData.MoneyAddress[mainContext.veridx]);
                if (newmoney - lastmoney == 17500)
                {
                    sendsplit();
                    DebugLog("get 17500 en, split");
                }
                lastmoney = newmoney;
            }

            #endregion

            int mapid = MemoryHelper.GetMemoryValue<int>(process, StaticData.MapAddress[mainContext.veridx]);
            if (lastmapid != mapid)
            {
                DebugLog("newmap: " + mapid + ":" + StaticData.MapNames[mapid]);
                lastmapid = mapid;
            }


            #region checkTM



            #endregion

            UpdateDebugArea(process);
        }

        private void UpdateDebugArea(Process process)
        {
            int ptr = MemoryHelper.GetMemoryValue<int>(process, StaticData.EnenyPtrAddr[mainContext.veridx]);
            //                    List<int> bosses = new List<int>();
            //                    List<int> HPS = new List<int>();
            //                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => debugContext.BossList.Clear()));
            //                    ptr += StaticData.EnenyEntitySize[mainContext.veridx] * 3;
            for (var i = 0; i < 50; i++)
            {
                ptr += StaticData.EnenyEntitySize[mainContext.veridx];
                debugContext.BossList[i].BossID = MemoryHelper.GetMemoryValue<int>(process,
                    ptr + StaticData.EnenyEnitiyIDOffset[mainContext.veridx], false);
                debugContext.BossList[i].BossHP = MemoryHelper.GetMemoryValue<int>(process,
                    ptr + StaticData.EnenyEnitiyHPOffset[mainContext.veridx], false);
            }

            debugContext.BossEvent = bossbattle;
        }

        private void DebugLog(string log)
        {
            this.debugContext.Log(log);
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
