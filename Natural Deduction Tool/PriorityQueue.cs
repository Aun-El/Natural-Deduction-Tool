using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natural_Deduction_Tool
{
    public class PriorityQueue
    {
        public List<Derivation> Queue { get; private set; }

        public PriorityQueue()
        {
            Queue = new List<Derivation>();
        }

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
                            //The derivation already exists with lower length
                            return;
                        }
                        else
                        {
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

        public Derivation Dequeue()
        {
            Derivation output = Queue.First();
            Queue.Remove(output);
            return output;
        }

        public bool Any()
        {
            return Queue.Any();
        }
    }
}
