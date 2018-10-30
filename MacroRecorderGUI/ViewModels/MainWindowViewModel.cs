﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using RecordPlaybackDLLEnums;
using MacroRecorderGUI.Commands;
using MacroRecorderGUI.Models;
using ProtobufGenerated;

namespace MacroRecorderGUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            MacroTabs = new ObservableCollection<MacroTab> {new MacroTab(new Macro(), "macro0"), new MacroTab(new Macro(), "macro1")};
            InitRecordEngine();
        }

        public class MacroTab
        {
            public Macro Macro { get; }

            public MacroTab(Macro macro, string name)
            {
                Macro = macro;
                Name = name;
            }

            public string Name { get; set; }
            
            private ICommand _closeTabCommand;

            public ICommand CloseTabCommand
            {
                get
                {
                    return _closeTabCommand ?? (_closeTabCommand =
                               new DelegateCommand<ObservableCollection<MacroTab>>(macroTabs => macroTabs.Remove(this)));
                }
            }

        }
        public ObservableCollection<MacroTab> MacroTabs { get; set; }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set { _selectedTabIndex = value; OnPropertyChanged();}
        }

        public bool LoopPlayback { get; set; }
        public Macro ActiveMacro => MacroTabs[SelectedTabIndex].Macro;


        public void AddNewTab()
        {
            MacroTabs.Add(new MacroTab(new Macro(), $"macro{MacroTabs.Count}"));
            SelectedTabIndex = MacroTabs.Count - 1;
        }


        #region RecordEngineStuff
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private RecordPlaybackDll.RecordEventCallback _recordEventCallbackDelegate;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private RecordPlaybackDll.StatusCallback _statusCallbackDelegate;
        private int _selectedTabIndex;

        private void RecordEventCb(IntPtr evtBufPtr, int bufSize)
        {
            var evtBuf = new byte[bufSize];
            Marshal.Copy(evtBufPtr, evtBuf, 0, bufSize);
            var parsedEvent = ProtobufGenerated.InputEvent.Parser.ParseFrom(evtBuf);

            Application.Current.Dispatcher.Invoke(()=> ActiveMacro.AddEvent(parsedEvent));
        }

        private void StatusCb(RecordPlaybackDLLEnums.StatusCode statusCode)
        {
            if (statusCode == StatusCode.PlaybackFinished)
            {
                //TODO: publish an event here?
                if (LoopPlayback) ActiveMacro.PlayMacro();
            }
            else
            {
                MessageBox.Show("Status reported: \"" + statusCode + "\".");
            }
        }

        private void InitRecordEngine()
        {
            _statusCallbackDelegate = StatusCb;
            _recordEventCallbackDelegate = RecordEventCb;

            RecordPlaybackDll.Init(_recordEventCallbackDelegate, _statusCallbackDelegate);
        }
    }
    #endregion
}