﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MacroRecorderGUI.ViewModels;
using ProtobufGenerated;

namespace MacroRecorderGUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartRecord_Click(object sender, RoutedEventArgs e)
        {
            if (ClearListOnStartRecord.IsChecked == true) ClearList_Click(sender, e);
            RecordPlaybackDll.StartRecord();
        }

        private void StopRecord_Click(object sender, RoutedEventArgs e)
        {
            RecordPlaybackDll.StopRecord();
            if (AutoChangeDelay.IsChecked == true) ChangeDelays_Click(sender, e);
        }

        private void PlayEvents_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as MainWindowViewModel)?.ActiveMacro?.PlayMacro();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TODO: the need to handle this manually disgusts me, however there's no native way in wpf to bind to SelectedItems. another possible WA:
            // when Events will be wrapped in a presentable class, it can have a "isSelected" property which is bound per item in the list
            // (not sure about the overhead of such a mass binding)
            foreach (ProtobufGenerated.InputEvent addedItem in e.AddedItems)
            {
                ((sender as ListBox)?.DataContext as MacroViewModel)?.SelectedEvents.Add(addedItem);
            }
            foreach (ProtobufGenerated.InputEvent removedItem in e.RemovedItems)
            {
                ((sender as ListBox)?.DataContext as MacroViewModel)?.SelectedEvents.Remove(removedItem);
            }
        }

        private void RemoveEvent_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as MainWindowViewModel)?.ActiveMacro?.RemoveSelectedEvents();
        }

        private void EventsListBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                RemoveEvent_Click(sender, e);
            }
        }

        private void ClearList_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as MainWindowViewModel)?.ActiveMacro?.Clear();
        }

        private void AllowOnlyNumbersInTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ChangeDelays_Click(object sender, RoutedEventArgs e)
        {
            if (!DelayTextBox.Text.Any()) return;
            var timeIncrement = Convert.ToUInt64(DelayTextBox.Text);
            (DataContext as MainWindowViewModel)?.ActiveMacro?.ChangeDelays(timeIncrement);
        }
        
        private void SaveEvents_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as MainWindowViewModel)?.ActiveMacro?.SaveToFile();
        }

        private void LoadEvents_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as MainWindowViewModel)?.AddNewTab();
            (DataContext as MainWindowViewModel)?.ActiveMacro?.LoadFromFile();
        }

        private void AbortPlayback_Click(object sender, RoutedEventArgs e)
        {
            RecordPlaybackDll.PlaybackEventAbort();
        }

        private void AddTab_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as MainWindowViewModel)?.AddNewTab();
        }

        private void CaptureEvent_Click(object sender, RoutedEventArgs e)
        {
            //var NewWindow = new CaptureEventWindow();
            //NewWindow.Show();
            SimulatedMouseEvent.MousePoint position = SimulatedMouseEvent.GetCursorPosition();
            InputEvent virtualMouse = new InputEvent
            {
                MouseEvent = new InputEvent.Types.MouseEventType
                {
                    X = position.X,
                    Y = position.Y,
                    ActionType = (int)SimulatedMouseEvent.MouseEventFlags.Absolute,
                },
                TimeSinceStartOfRecording = GetCuurentTimestamp((DataContext as MainWindowViewModel)?.ActiveMacro?.Events)
            };
            (DataContext as MainWindowViewModel)?.ActiveMacro?.AddEvent(virtualMouse);
            var NewWindow = new CaptureEventWindow();
            NewWindow.Show();

            //this.KeyDown += new KeyEventHandler(Form1_KeyEvent);
            //this.KeyUp += new KeyEventHandler(Form1_KeyEvent);
            //this.MouseEnter += new MouseEventHandler(MouseRecorder_ActionRecorded);
        }
        private void MouseRecorder_ActionRecorded(object sender, MouseEventArgs e)
        {
            //InputEvent virtualMouse = new InputEvent
            //{
            //    MouseEvent = new InputEvent.Types.MouseEventType
            //    {
            //        X = System.Windows.Forms.Control.MousePosition.X,
            //        Y = System.Windows.Forms.Control.MousePosition.Y,
            //        ActionType = 0,
            //        WheelRotation = 0,
            //        RelativePosition = false,
            //        MappedToVirtualDesktop = false
            //    }, 
            //    TimeSinceStartOfRecording = 0
            //};
            //(DataContext as MainWindowViewModel)?.ActiveMacro?.AddEvent(virtualMouse);
            //this.MouseEnter -= new MouseEventHandler(MouseRecorder_ActionRecorded);
        }
        static public ulong GetCuurentTimestamp(ObservableCollection<InputEvent> events)
        {
            return events.Count > 0 ? events[events.Count - 1].TimeSinceStartOfRecording : 0;
        }
        void Form1_KeyEvent(object sender, KeyEventArgs e)
        {
      
            InputEvent capturedKey = new InputEvent
            {
                KeyboardEvent = new InputEvent.Types.KeyboardEventType
                {
                    KeyUp = e.IsUp,
                    VirtualKeyCode = Convert.ToUInt32(KeyInterop.VirtualKeyFromKey(e.Key))
                },
                TimeSinceStartOfRecording = GetCuurentTimestamp((DataContext as MainWindowViewModel)?.ActiveMacro?.Events)
            };
            if(e.IsUp == true)
            {
                this.KeyUp -= new KeyEventHandler(Form1_KeyEvent);
                this.KeyDown -= new KeyEventHandler(Form1_KeyEvent);
            }
            (DataContext as MainWindowViewModel)?.ActiveMacro?.AddEvent(capturedKey);
            
        }
        
    }
}
