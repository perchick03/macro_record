﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using Google.Protobuf;
using MacroRecorderGUI.Commands;
using MacroRecorderGUI.Utils;
using ProtobufGenerated;

namespace MacroRecorderGUI
{
    public class Macro
    {
        public Macro(string name)
        {
            Name = name;
        }

        public ObservableCollection<InputEvent> Events { get; } = new ObservableCollection<InputEvent>();

        public string Name { get; set; }
            
        private ICommand _closeTabCommand;
        public ICommand CloseTabCommand
        {
            get
            {
                return _closeTabCommand ?? (_closeTabCommand =
                           new DelegateCommand<ObservableCollection<Macro>>(macroTabs => macroTabs.Remove(this)));
            }
        }

        internal static byte[] SerializeEventsToByteArray(IEnumerable<InputEvent> inputEventList)
        {
            var serializedEvents = new InputEventList();
            serializedEvents.InputEvents.AddRange(inputEventList);
            var serializedEventsByteArray = serializedEvents.ToByteArray();
            return serializedEventsByteArray;
        }

        internal static IEnumerable<InputEvent> DeserializeEventsFromByteArray(byte[] serializedEvents)
        {
            var deserializedEvents = InputEventList.Parser.ParseFrom(serializedEvents);
            return deserializedEvents.InputEvents;
        }

        public void PlayMacro()
        {
            if (!Events.Any()) return;
            var eventsWrappedWithReleasingModKeys = ReleaseModifierKeys.ReleaseModKeysEvents
                .Concat(Events).Concat(ReleaseModifierKeys.ReleaseModKeysEvents);
            var serializedEventsByteArray = SerializeEventsToByteArray(eventsWrappedWithReleasingModKeys);
            RecordPlaybackDll.PlaybackEvents(serializedEventsByteArray);
        }

        public void Clear()
        {
            Events.Clear();
        }

        public List<InputEvent> SelectedEvents { get; set; } = new List<InputEvent>();
        public void RemoveSelectedEvents()
        {
            var copyOfInputEvents = SelectedEvents.ToList();
            foreach (var eventToRemove in copyOfInputEvents)
            {
                Events.Remove(eventToRemove);
            }
        }

        public void ChangeDelays(ulong timeIncrement)
        {
            var currentTimeOffset = 0ul;
            foreach (var inputEvent in Events)
            {
                inputEvent.TimeSinceStartOfRecording = currentTimeOffset;
                currentTimeOffset += timeIncrement;
            }

            //TODO: implement the events as wrapper class around protobuf class, and implement PropertyChanged event listeners on them
            CollectionViewSource.GetDefaultView(Events).Refresh();
        }

        public void SaveToFile()
        {
            FileOperations.SaveEventsToFile(Events);
        }

        public void LoadFromFile()
        {
            var deserializedEvents = FileOperations.LoadEventsFromFile();
            if (deserializedEvents != null)
            {
                PopulateEventCollectionWithNewEvents(deserializedEvents);
            }
        }

        public void PopulateEventCollectionWithNewEvents(IEnumerable<InputEvent> deserializedEvents)
        {
            Events.Clear();
            foreach (var deserializedEvent in deserializedEvents)
            {
                Events.Add(deserializedEvent);
            }
        }

        public void AddEvent(InputEvent parsedEvent)
        {
            Events.Add(parsedEvent);
        }
    }
}