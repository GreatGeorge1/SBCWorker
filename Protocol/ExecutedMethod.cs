using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Protocol
{
    public class ExecutedMethod : INotifyPropertyChanged
    {
        public Method MethodInfo { get; set; }
        private string commandValue;
        public string CommandValue
        {
            get { return commandValue; }
            set
            {
                commandValue = value;
                OnPropertyChanged("CommandValue");
            }
        }
        private ResponseHeader responseHeader;
        public ResponseHeader ResponseHeader
        {
            get { return responseHeader; }
            set
            {
                responseHeader = value;
                OnPropertyChanged("ResponseHeader");
            }
        }
        private string responseValue;
        public string ResponseValue
        {
            get { return this.responseValue; }
            set
            {
                responseValue = value;
                OnPropertyChanged("ResponseValue");
            }
        }
        private string hash;
        public string Hash
        {
            get { return hash; }
            set
            {
                hash = value;
                OnPropertyChanged("Hash");
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

        public bool IsError { get; set; }

        private int repeatCount;

        public ExecutedMethod()
        {
            repeatCount = 0;
        }

        public int RepeatCount
        {
            get { return repeatCount; }
            set { repeatCount = value; ValidateLimit(value); }
        }

        public int RepeatLimit { get; set; }

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

        public string CreateResponse()
        {
            var command = this.ResponseHeader;
            return $"{command.GetDisplayName()}\r\n";
        }
    }
}
