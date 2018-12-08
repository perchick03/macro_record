﻿using MacroRecorderGUI.ViewModels;
using ProtobufGenerated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MacroRecorderGUI
{
    /// <summary>
    /// Interaction logic for CaptureEventWindow.xaml
    /// </summary>
    public partial class CaptureEventWindow : Window
    {
        public CaptureEventWindow()
        {
            InitializeComponent();
            MouseXTextBox.Text = "X";
            MouseYTextBox.Text = "Y";

        }
        static private uint GetMouseAction(string MouseAction)
        {
            switch(MouseAction)
            {
                case "MouseMove":
                    return 0x0001;
                case "LeftDown":
                    return 0x0001;
                case "LeftUp":
                    return 0x0004;
                case "RightDown":
                    return 0x0008;
                case "RightUp":
                    return 0x0010;
                case "MiddleDown":
                    return 0x0020;
                case "MiddleUp":
                    return 0x0040;
                case "XDown":
                    return 0x0080;
                case "XUp":
                    return 0x0100;
                default:
                    return 0;
            }
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string SelectedItem = ((ComboBoxItem)ComboBoxEvent.SelectedItem).Content.ToString();
            
            if (SelectedItem == "Mouse Event")
            {               
                MouseXLabel.Visibility = Visibility.Visible;
                MouseYLabel.Visibility = Visibility.Visible;

                MouseXTextBox.Visibility = Visibility.Visible;
                MouseYTextBox.Visibility = Visibility.Visible;

                ActionLabel.Visibility = Visibility.Visible;
                ActionType.Visibility = Visibility.Visible;
            }
            
        }

        private void ActionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //MessageBox.Show(MouseXTextBox.Text);
            //InputEvent virtualMouse = new InputEvent
            //{
            //    MouseEvent = new InputEvent.Types.MouseEventType
            //    {
            //        X = 0, //Convert.ToInt32(MouseXTextBox.Text),
            //        Y = 0,  //Convert.ToInt32(MouseYTextBox.Text),
            //        ActionType = 0 //GetMouseAction(((ComboBoxItem)ActionType.SelectedItem).Content.ToString())
            //    },
            //    TimeSinceStartOfRecording = 0 //MainWindow.GetCuurentTimestamp((DataContext as MainWindowViewModel)?.ActiveMacro?.Events)
            //};
            //(DataContext as MainWindowViewModel)?.ActiveMacro?.AddEvent(virtualMouse);
        }

        private void EnterEvent_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(MouseXTextBox.Text) && !string.IsNullOrEmpty(MouseYTextBox.Text)&& (ActionType.SelectedIndex != -1))
            {
                
                InputEvent virtualMouse = new InputEvent
                {
                    MouseEvent = new InputEvent.Types.MouseEventType
                    {
                        X = Convert.ToInt32(MouseXTextBox.Text),
                        Y = Convert.ToInt32(MouseYTextBox.Text),
                        ActionType = GetMouseAction(((ComboBoxItem)ActionType.SelectedItem).Content.ToString())
                    },
                    TimeSinceStartOfRecording = MainWindow.GetCuurentTimestamp(MainWindow.MainWindoeInstanceForMouseEvent.ActiveMacro.Events)
                };
                MainWindow.MainWindoeInstanceForMouseEvent.ActiveMacro.AddEvent(virtualMouse);
                this.Close();
            }
        }
    }
}