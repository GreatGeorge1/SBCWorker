﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Protocol
{
    public class ExecutedMethod : INotifyPropertyChanged
    {
        public Method MethodInfo { get; set; }
        private byte[] commandValue;
        public byte[] CommandValue
        {
            get { return commandValue; }
            set
            {
                commandValue = value;
                OnPropertyChanged("CommandValue");
            }
        }

        private byte[] responseValue;
        public byte[] ResponseValue
        {
            get { return this.responseValue; }
            set
            {
                responseValue = value;
                OnPropertyChanged("ResponseValue");
            }
        }
        private bool isFired;
        public bool IsFired
        {
            get { return isFired; }
            set { isFired = value; OnPropertyChanged("IsFired"); }
        }
        private bool isCompleted;
        public bool IsCompleted
        {
            get { return isCompleted; }
            set { isCompleted = value; OnPropertyChanged("IsCompleted"); }
        }


        private List<byte[]> errors;
        public bool IsError { get; set; }

        private int repeatCount;

        public ExecutedMethod()
        {
            repeatCount = 0;
            errors = new List<byte[]>();
            OnPropertyChanged("Init");
        }

        public int RepeatCount
        {
            get { return repeatCount; }
            set { repeatCount = value; ValidateLimit(value); }
        }

        public int RepeatLimit { get; set; }

        public void PushError(byte[] value)
        {
            IsError = true;
            errors.Add(value);
        }

        private void ValidateLimit(int count)
        {
            if (count >= this.RepeatLimit)
            {
                OnRepeatCountReachedLimit(count, this.RepeatLimit);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public event RepeatCountReachedLimitEventHandler RepeatCountReachedLimit;


        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        protected void OnRepeatCountReachedLimit(int count, int limit)
        {

            RepeatCountReachedLimitEventHandler handler = RepeatCountReachedLimit;
            if (handler != null)
            {
                handler(this, new RepeatCountReachedLimitArgs(count, limit));
            }
        }

        public byte[] CreateResponse()
        {
            //var command = this.ResponseHeader;
            //TODO implement
            //return $"{command.GetDisplayName()}\r\n";
            byte[] msg = { 0x02, 0xD6, 0xC7, 0x00, 0x01, 0x01, 0x03, 0x0d,0x0a };

            //return $"{Encoding.ASCII.GetString(msg)}\r\n";
            return msg;
        }
    }
}
