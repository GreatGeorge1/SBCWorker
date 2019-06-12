using ConsoleTableExt;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Worker.Host;

namespace DevConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //DataTable data = new DataTable();
            //data.Columns.Add("CommandHeader", typeof(ProtocolCommands));
            //data.Columns.Add("ResponseHeaders", typeof(ICollection<ProtocolResponse>));
            //data.Columns.Add("HasResponseHeader", typeof(bool));
            //data.Columns.Add("HasResponseValue", typeof(bool));
            //data.Columns.Add("HasCommandValue", typeof(bool));
            //data.Columns.Add("IsHashable", typeof(bool));
            //data.Columns.Add("IsControllerHosted", typeof(bool));
            var list = new List<ProtocolMethodView>();
            foreach(var item in Protocol.Methods)
            {
                list.Add(ProtocolMethodView.MapProtocolMethod(item.Value));
            }
            ConsoleTableBuilder.From(list).WithFormat(ConsoleTableBuilderFormat.Minimal).ExportAndWriteLine();

            CustomQueue<string> cqueue = new CustomQueue<string>();
            cqueue.EnqueueEvent += EnqueueAction;
            cqueue.Enqueue("item");
            cqueue.Enqueue("item2");
            Console.WriteLine(cqueue.Count);
        }
        public static async void EnqueueAction(object sender, CustomQueueEnqueueEventArgs<string> e)
        {
            if (!String.IsNullOrWhiteSpace(e.Item))
            {
                Console.WriteLine($"Enqueue:{e.Item}");
            }
            else
            {
                Console.WriteLine("EnqueueAction error");
            }
        }
    }

    class ProtocolMethodView : ProtocolMethod
    {
        public new string ResponseHeaders { get; set; }
        public static ProtocolMethodView MapProtocolMethod(ProtocolMethod input)
        {
            var res = new ProtocolMethodView
            {
                CommandHeader=input.CommandHeader,
                HasCommandValue=input.HasCommandValue,
                HasResponseHeader=input.HasResponseHeader,
                HasResponseValue=input.HasResponseValue,
                IsControllerHosted=input.IsControllerHosted,
                IsHashable=input.IsHashable
            };
            string responseHeaders = "";
            if(input.ResponseHeaders!=null && input.ResponseHeaders.Any())
            {
                foreach(var item in input.ResponseHeaders)
                {
                    responseHeaders += $"{item.ToString()}; ";
                }
            }
            res.ResponseHeaders = responseHeaders;

            return res;
        }
    }

}
