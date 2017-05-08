using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using rabi_splitter_WPF.Annotations;

namespace rabi_splitter_WPF
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public class EntityStatsData : INotifyPropertyChanged
    {
        private int _addr;
        private int _intval;
        private float _floatval;

        public int Addr
        {
            get { return _addr; }
            set
            {
                if (value == _addr) return;
                _addr = value;
                OnPropertyChanged(nameof(Addr));
            }
        }

        public int IntVal
        {
            get { return _intval; }
            set
            {
                if (value == _intval) return;
                _intval = value;
                OnPropertyChanged(nameof(IntVal));
            }
        }

        public float FloatVal
        {
            get { return _floatval; }
            set
            {
                if (value == _floatval) return;
                _floatval = value;
                OnPropertyChanged(nameof(FloatVal));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BossData:INotifyPropertyChanged
    {
        private int _bossIdx;
        private int _bossId;
        private int _bossHp;

        public int BossIdx
        {
            get { return _bossIdx; }
            set
            {
                if (value == _bossIdx) return;
                _bossIdx = value;
                OnPropertyChanged(nameof(BossIdx));
            }
        }

        public int BossID
        {
            get { return _bossId; }
            set
            {
                if (value == _bossId) return;
                _bossId = value;
                OnPropertyChanged(nameof(BossID));
            }
        }

        public int BossHP
        {
            get { return _bossHp; }
            set
            {
                if (value == _bossHp) return;
                _bossHp = value;
                OnPropertyChanged(nameof(BossHP));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    class DebugContext : INotifyPropertyChanged
    {
        private int _entityAnalysisIndex;
        private bool _bossEvent;
        private string _debugLog;

        public ObservableCollection<BossData> BossList = new ObservableCollection<BossData>();
        public ObservableCollection<EntityStatsData> EntityStatsListData = new ObservableCollection<EntityStatsData>();
        public int targetEntityListSize;

        public ObservableCollection<EntityStatsData> EntityStatsListView
        {
            get
            {
                return EntityStatsListData;
            }
        }

        public DebugContext()
        {
            this.EntityAnalysisIndex = 0;

            BossList =new ObservableCollection<BossData>();
            for (int i = 0; i < 50; i++)
            {
                BossList.Add(new BossData()
                {
                    BossIdx = i
                });
            }

            while (EntityStatsListData.Count < 449)
            {
                EntityStatsListData.Add(new EntityStatsData() { Addr = EntityStatsListData.Count * 4 });
            }
        }

        public int EntityAnalysisIndex
        {
            get { return _entityAnalysisIndex; }
            set
            {
                if (value == _entityAnalysisIndex) return;
                _entityAnalysisIndex = value;
                OnPropertyChanged(nameof(EntityAnalysisIndex));
            }
        }

        public bool BossEvent
        {
            get { return _bossEvent; }
            set
            {
                if (value == _bossEvent) return;
                _bossEvent = value;
                OnPropertyChanged(nameof(BossEvent));
            }
        }

        public string DebugLog
        {
            get { return _debugLog; }
            set
            {
                if (value == _debugLog) return;
                _debugLog = value;
                OnPropertyChanged(nameof(DebugLog));
            }
        }

        public void Log(string message)
        {
            this.DebugLog += message + "\n";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    class MainContext : INotifyPropertyChanged
    {
        public string oldtitle;
        public int veridx;
        
        private int _serverPort;
        private string _gameVer;
        private string _gameMusic;
        private bool _igt;

        private string _text1;
        private string _text2;
        private string _text3;
        private string _text4;
        private string _text5;
        private string _text6;
        private string _text7;
        private string _text8;
        private string _text9;
        private string _text10;
        private string _text11;
        private string _text12;
        private string _text13;
        private string _text14;
        private string _text15;
        private string _text16;
        private string _text17;
        private string _text18;
        private string _text19;
        private string _text20;

        public string Text1
        {
            get { return _text1; }
            set
            {
                if (value == _text1) return;
                _text1 = value;
                OnPropertyChanged(nameof(Text1));
            }
        }

        public string Text2
        {
            get { return _text2; }
            set
            {
                if (value == _text2) return;
                _text2 = value;
                OnPropertyChanged(nameof(Text2));
            }
        }

        public string Text3
        {
            get { return _text3; }
            set
            {
                if (value == _text3) return;
                _text3 = value;
                OnPropertyChanged(nameof(Text3));
            }
        }

        public string Text4
        {
            get { return _text4; }
            set
            {
                if (value == _text4) return;
                _text4 = value;
                OnPropertyChanged(nameof(Text4));
            }
        }

        public string Text5
        {
            get { return _text5; }
            set
            {
                if (value == _text5) return;
                _text5 = value;
                OnPropertyChanged(nameof(Text5));
            }
        }

        public string Text6
        {
            get { return _text6; }
            set
            {
                if (value == _text6) return;
                _text6 = value;
                OnPropertyChanged(nameof(Text6));
            }
        }

        public string Text7
        {
            get { return _text7; }
            set
            {
                if (value == _text7) return;
                _text7 = value;
                OnPropertyChanged(nameof(Text7));
            }
        }

        public string Text8
        {
            get { return _text8; }
            set
            {
                if (value == _text8) return;
                _text8 = value;
                OnPropertyChanged(nameof(Text8));
            }
        }

        public string Text9
        {
            get { return _text9; }
            set
            {
                if (value == _text9) return;
                _text9 = value;
                OnPropertyChanged(nameof(Text9));
            }
        }

        public string Text10
        {
            get { return _text10; }
            set
            {
                if (value == _text10) return;
                _text10 = value;
                OnPropertyChanged(nameof(Text10));
            }
        }

        public string Text11
        {
            get { return _text11; }
            set
            {
                if (value == _text11) return;
                _text11 = value;
                OnPropertyChanged(nameof(Text11));
            }
        }

        public string Text12
        {
            get { return _text12; }
            set
            {
                if (value == _text12) return;
                _text12 = value;
                OnPropertyChanged(nameof(Text12));
            }
        }

        public string Text13
        {
            get { return _text13; }
            set
            {
                if (value == _text13) return;
                _text13 = value;
                OnPropertyChanged(nameof(Text13));
            }
        }

        public string Text14
        {
            get { return _text14; }
            set
            {
                if (value == _text14) return;
                _text14 = value;
                OnPropertyChanged(nameof(Text14));
            }
        }

        public string Text15
        {
            get { return _text15; }
            set
            {
                if (value == _text15) return;
                _text15 = value;
                OnPropertyChanged(nameof(Text15));
            }
        }

        public string Text16
        {
            get { return _text16; }
            set
            {
                if (value == _text16) return;
                _text16 = value;
                OnPropertyChanged(nameof(Text16));
            }
        }

        public string Text17
        {
            get { return _text17; }
            set
            {
                if (value == _text17) return;
                _text17 = value;
                OnPropertyChanged(nameof(Text17));
            }
        }

        public string Text18
        {
            get { return _text18; }
            set
            {
                if (value == _text18) return;
                _text18 = value;
                OnPropertyChanged(nameof(Text18));
            }
        }

        public string Text19
        {
            get { return _text19; }
            set
            {
                if (value == _text19) return;
                _text19 = value;
                OnPropertyChanged(nameof(Text19));
            }
        }

        public string Text20
        {
            get { return _text20; }
            set
            {
                if (value == _text20) return;
                _text20 = value;
                OnPropertyChanged(nameof(Text20));
            }
        }

        public int ServerPort
        {
            get { return _serverPort; }
            set
            {
                if (value == _serverPort) return;
                _serverPort = value;
                OnPropertyChanged(nameof(ServerPort));
            }
        }

        public string GameVer
        {
            get { return _gameVer; }
            set
            {
                if (value == _gameVer) return;
                _gameVer = value;
                OnPropertyChanged(nameof(GameVer));
            }
        }

        public string GameMusic
        {
            get { return _gameMusic; }
            set
            {
                if (value == _gameMusic) return;
                _gameMusic = value;
                OnPropertyChanged(nameof(GameMusic));
            }
        }

        public bool Igt
        {
            get { return _igt; }
            set
            {
                if (value == _igt) return;
                _igt = value;
                OnPropertyChanged(nameof(Igt));
            }
        }
        
        public MainContext()
        {
            this.ServerPort = 16834;
            this.Igt = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

