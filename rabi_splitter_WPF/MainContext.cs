using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using rabi_splitter_WPF.Annotations;
using System.Windows;

namespace rabi_splitter_WPF
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InvertableBooleanToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members
        // Code taken from http://stackoverflow.com/a/2427307
        enum Parameter
        {
            VisibleWhenTrue, VisibleWhenFalse
        }

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            var boolValue = (bool)value;
            var direction = (Parameter)Enum.Parse(typeof(Parameter), (string)parameter);

            if (direction == Parameter.VisibleWhenTrue) return boolValue ? Visibility.Visible : Visibility.Collapsed;
            else return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return null;
        }
        #endregion
    }

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
        private bool _bossEvent;
        private string _debugLog;

        public ObservableCollection<BossData> BossList = new ObservableCollection<BossData>();

        public DebugContext()
        {
            BossList=new ObservableCollection<BossData>();
            for (int i = 0; i < 50; i++)
            {
                BossList.Add(new BossData()
                {
                    BossIdx = i
                });
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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    class MainContext : INotifyPropertyChanged
    {
        #region _general
        public bool _autoStart;
        public bool _autoReset;
        public bool _advancedSettings;
        private bool _practiceMode;
        private bool _debugMode;
        private bool _dontSplitOnReload;
        #endregion

        #region _splits
        private bool _splitOnCarrotBomb;
        private bool _splitOnRiverbank;
        private bool _splitOnEvernight;
        private bool _splitOnSaya;
        private bool _splitOnAquarium;

        private bool _splitOnKotri1;
        private bool _splitOnRita;
        private bool _splitOnVanilla;
        private bool _splitOnTown;
        private bool _splitOnAlius3;

        private bool _splitOnHospital;
        private bool _splitOnMiru;
        private bool _splitOnBalcony;
        private bool _splitOnNoah1;
        private bool _splitOnNoah3;
        #endregion

        #region _unsorted
        private bool _musicStart;
        private bool _musicEnd;
        private bool _miruDe;
        private bool _tm2;
        private bool _alius1;
        #endregion

        private int _serverPort;
        private string _gameVer;
        private string _gameMusic;
        private bool _igt;

        public string BOSSTimer
        {
            get
            {
                if (LastBossStart == null) return "N/A";
                else
                {
                    var dt = DateTime.Now;
                    if (LastBossEnd.HasValue && LastBossStart.Value<LastBossEnd.Value)
                    {
                        dt = LastBossEnd.Value;
                    }
                    var ts = dt- LastBossStart.Value;
                    return ts.ToString(@"mm\:ss\.f");
                }
                
            }
        }
        public string RoutingTimer
        {
            get
            {
                if (LastBossEnd == null) return "N/A";
                else
                {
                    var dt = DateTime.Now;
                    if (LastBossStart.HasValue && LastBossStart.Value > LastBossEnd.Value)
                    {
                        dt = LastBossStart.Value;
                    }
                    var ts = dt - LastBossEnd.Value;
                    return ts.ToString(@"mm\:ss\.f");
                }
            }
        }
        public DateTime? LastBossStart;
        public DateTime? LastBossEnd;

        #region General
        public bool AutoStart
        {
            get { return _autoStart; }
            set
            {
                if (value == _autoStart) return;
                _autoStart = value;
                OnPropertyChanged(nameof(AutoStart));
            }
        }
        public bool AutoReset
        {
            get { return _autoReset; }
            set
            {
                if (value == _autoReset) return;
                _autoReset = value;
                OnPropertyChanged(nameof(AutoReset));

            }
        }
        public bool AdvancedSettings
        {
            get { return _advancedSettings; }
            set
            {
                if (value == _advancedSettings) return;
                _advancedSettings = value;
                OnPropertyChanged(nameof(AdvancedSettings));
            }
        }
        public bool PracticeMode
        {
            get { return _practiceMode; }
            set
            {
                if (value == _practiceMode) return;
                _practiceMode = value;
                OnPropertyChanged(nameof(PracticeMode));
            }
        }
        public bool DebugMode
        {
            get { return _debugMode; }
            set
            {
                if (value == _debugMode) return;
                _debugMode = value;
                OnPropertyChanged(nameof(DebugMode));
            }
        }
        public bool DontSplitOnReload
        {
            get { return _dontSplitOnReload; }
            set
            {
                if (value == _dontSplitOnReload) return;
                _dontSplitOnReload = value;
                OnPropertyChanged(nameof(DontSplitOnReload));
            }
        }
        #endregion

        #region Splits
        public bool SplitOnCarrotBomb
        {
            get { return _splitOnCarrotBomb; }
            set
            {
                if (value == _splitOnCarrotBomb) return;
                _splitOnCarrotBomb = value;
                OnPropertyChanged(nameof(SplitOnCarrotBomb));
            }
        }
        public bool SplitOnRiverbank
        {
            get { return _splitOnRiverbank; }
            set
            {
                if (value == _splitOnRiverbank) return;
                _splitOnRiverbank = value;
                OnPropertyChanged(nameof(SplitOnRiverbank));
            }
        }
        public bool SplitOnEvernight
        {
            get { return _splitOnEvernight; }
            set
            {
                if (value == _splitOnEvernight) return;
                _splitOnEvernight = value;
                OnPropertyChanged(nameof(SplitOnEvernight));
            }
        }
        public bool SplitOnSaya
        {
            get { return _splitOnSaya; }
            set
            {
                if (value == _splitOnSaya) return;
                _splitOnSaya = value;
                OnPropertyChanged(nameof(SplitOnSaya));
            }
        }
        public bool SplitOnAquarium
        {
            get { return _splitOnAquarium; }
            set
            {
                if (value == _splitOnAquarium) return;
                _splitOnAquarium = value;
                OnPropertyChanged(nameof(SplitOnAquarium));
            }
        }
        public bool SplitOnKotri1
        {
            get { return _splitOnKotri1; }
            set
            {
                if (value == _splitOnKotri1) return;
                _splitOnKotri1 = value;
                OnPropertyChanged(nameof(SplitOnKotri1));
            }
        }
        public bool SplitOnRita
        {
            get { return _splitOnRita; }
            set
            {
                if (value == _splitOnRita) return;
                _splitOnRita = value;
                OnPropertyChanged(nameof(SplitOnRita));
            }
        }
        public bool SplitOnVanilla
        {
            get { return _splitOnVanilla; }
            set
            {
                if (value == _splitOnVanilla) return;
                _splitOnVanilla = value;
                OnPropertyChanged(nameof(SplitOnVanilla));
            }
        }
        public bool SplitOnTown
        {
            get { return _splitOnTown; }
            set
            {
                if (value == _splitOnTown) return;
                _splitOnTown = value;
                OnPropertyChanged(nameof(SplitOnTown));
            }
        }
        public bool SplitOnAlius3
        {
            get { return _splitOnAlius3; }
            set
            {
                if (value == _splitOnAlius3) return;
                _splitOnAlius3 = value;
                OnPropertyChanged(nameof(SplitOnAlius3));
            }
        }
        public bool SplitOnHospital
        {
            get { return _splitOnHospital; }
            set
            {
                if (value == _splitOnHospital) return;
                _splitOnHospital = value;
                OnPropertyChanged(nameof(SplitOnHospital));
            }
        }
        public bool SplitOnMiru
        {
            get { return _splitOnMiru; }
            set
            {
                if (value == _splitOnMiru) return;
                _splitOnMiru = value;
                OnPropertyChanged(nameof(SplitOnMiru));
            }
        }
        public bool SplitOnBalcony
        {
            get { return _splitOnBalcony; }
            set
            {
                if (value == _splitOnBalcony) return;
                _splitOnBalcony = value;
                OnPropertyChanged(nameof(SplitOnBalcony));
            }
        }
        public bool SplitOnNoah1
        {
            get { return _splitOnNoah1; }
            set
            {
                if (value == _splitOnNoah1) return;
                _splitOnNoah1 = value;
                OnPropertyChanged(nameof(SplitOnNoah1));
            }
        }
        public bool SplitOnNoah3
        {
            get { return _splitOnNoah3; }
            set
            {
                if (value == _splitOnNoah3) return;
                _splitOnNoah3 = value;
                OnPropertyChanged(nameof(SplitOnNoah3));
            }
        }
        #endregion


        #region Unsorted
        public bool Tm2
        {
            get { return _splitOnCarrotBomb; }
            set
            {
                if (value == _tm2) return;
                _tm2 = value;
                OnPropertyChanged(nameof(Tm2));
            }
        }
        public bool MiruDe
        {
            get { return _miruDe; }
            set
            {
                if (value == _miruDe) return;
                _miruDe = value;
                OnPropertyChanged(nameof(MiruDe));
            }
        }
        private bool _forceAlius1;
        public bool Alius1
        {
            get { return _alius1; }
            set
            {
                if (value == _alius1) return;
                _alius1 = value;
                OnPropertyChanged(nameof(Alius1));
            }
        }
        public bool Noah1Reload
        {
            get { return _noah1Reload; }
            set
            {
                if (value == _noah1Reload) return;
                _noah1Reload = value;
                OnPropertyChanged(nameof(Noah1Reload));
            }
        }
        public bool MusicStart
        {
            get { return _musicStart; }
            set
            {
                if (value == _musicStart) return;
                _musicStart = value;
                OnPropertyChanged(nameof(MusicStart));
            }
        }
        public bool MusicEnd
        {
            get { return _musicEnd; }
            set
            {
                if (value == _musicEnd) return;
                _musicEnd = value;
                OnPropertyChanged(nameof(MusicEnd));
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
        public bool ForceAlius1
        {
            get { return _forceAlius1; }
            set
            {
                if (value == _forceAlius1) return;
                _forceAlius1 = value;
                OnPropertyChanged(nameof(ForceAlius1));
            }
        }
        public bool Bossbattle
        {
            get { return bossbattle; }
            set
            {
                if (value == bossbattle) return;
                bossbattle = value;
                if (value)
                {
                    this.LastBossStart = DateTime.Now;
                }
                else
                {
                    this.LastBossEnd=DateTime.Now;
                }
               
               
            }
        }

        public int previousBlackness = -1;
        public string oldtitle;
        public int veridx;
        public int lastHasBomb;
        public int lastmapid;
        public int lastmusicid;
        public int lastplaytime = 0;
        public bool canReload = false; // set to true when playtime increases

        private bool bossbattle;
        public List<int> lastbosslist;
        public int lastnoah3hp;
        public int lastTM;
        public DateTime LastTMAddTime;
        private bool _noah1Reload;
        #endregion


        public MainContext()
        {
            //General Settings
            this.AutoStart = false;
            this.AutoReset = false;
            this.ServerPort = 16834;
            this.DebugMode = false;

            //Splits
            this.SplitOnCarrotBomb = true;
            this.SplitOnRiverbank = true;
            this.SplitOnEvernight = true;
            this.SplitOnSaya = true;
            this.SplitOnAquarium = true;

            this.SplitOnKotri1 = true;
            this.SplitOnRita = true;
            this.SplitOnVanilla = false;
            this.SplitOnTown = true;
            this.SplitOnAlius3 = true;

            this.SplitOnHospital = true;
            this.SplitOnMiru = true;
            this.SplitOnBalcony = true;
            this.SplitOnNoah1 = true;
            this.SplitOnNoah3 = true;

            //Other + Unsorted
            this.MusicEnd = true;
            this.MusicStart = false;
            this.MiruDe = true;
            this.Tm2 = true;
            this.Alius1 = true;    
            this.Igt = true;
            this.Noah1Reload = false;
            this.ForceAlius1 = false;
        }

        public void NotifyTimer()
        {
            OnPropertyChanged(nameof(BOSSTimer));
            OnPropertyChanged(nameof(RoutingTimer));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

