using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Natural_Deduction_Tool
{
    public class PriorityQueue
    {
        public List<Derivation> Queue { get; private set; }

        [DebuggerStepThrough]
        public PriorityQueue()
        {
            Queue = new List<Derivation>();
        }

        [DebuggerStepThrough]
        public void Enqueue(Derivation item,List<Derivation> closedList)
        {
            for (int i = 0; i < closedList.Count; i++)
            {
                if (item.Form.Equals(closedList[i].Form))
                {
                    //The derivation already exists
                    if(item.Length < closedList[i].Length)
                    {
                        closedList.RemoveAt(i);
                        closedList.Insert(i,item);
                    }
                    return;
                }
            }
            bool inserted = false;
            if (Queue.Count == 0)
            {
                Queue.Add(item);
            }
            else
            {
                for (int i = 0; i < Queue.Count; i++)
                {
                    if (Queue[i].Form.Equals(item.Form))
                    {
                        if (!inserted)
                        {
                            //The derivation already exists with lower or equal length
                            return;
                        }
                        else
                        {
                            //The derivation was already in the queue with a higher length
                            Queue.RemoveAt(i);
                            return;
                        }
                    }
                    if (!inserted && Queue[i].Length > item.Length)
                    {
                        inserted = true;
                        Queue.Insert(i, item);
                    }
                }
                if (!inserted)
                {
                    Queue.Add(item);
                }
            }
        }

        [DebuggerStepThrough]
        public Derivation Dequeue()
        {
            Derivation output = Queue.First();
            Queue.Remove(output);
            return output;
        }

        [DebuggerStepThrough]
        public bool Any()
        {
            return Queue.Any();
        }
    }
}
