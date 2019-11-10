using System;
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


        private readonly List<byte[]> errors;
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
        public string ResponseAddress { get; set; } //signalr

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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected void OnRepeatCountReachedLimit(int count, int limit)
        {

            RepeatCountReachedLimit?.Invoke(this, new RepeatCountReachedLimitArgs(count, limit));
        }

        public byte[] CreateResponse(byte[] result)
        {
            if(result is null)
            {
                throw new ArgumentNullException(nameof(result));
            }
            var list = new List<byte>
            {
                0x02,
                (byte)MessageType.RES,
                (byte)this.MethodInfo.CommandHeader,
                RequestMiddleware.CalCheckSum(result, result.Length),
            };
            list.AddRange(RequestMiddleware.IntToHighLow(result.Length));
            foreach (var item in result)
            {
                list.Add(item);
            }

            return list.ToArray();
        }
    }
}
