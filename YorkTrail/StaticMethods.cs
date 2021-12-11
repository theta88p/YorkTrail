using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YorkTrail
{
    public class StaticMethods
    {
        public static void ShallowCopy(IList<double> from, IList<double> to)
        {
            if (from != null && to != null)
            {
                to.Clear();

                foreach (var d in from)
                {
                    to.Add(d);
                }
            }
        }

        public static double GetClosePointInList(ObservableCollection<double>list, double position, ulong totalms, bool getPrev)
        {
            // 1000ms
            double alpha = 1.0 / totalms * 1000;

            for (int i = 0; i <= list.Count; i++)
            {
                double prev;
                double next;

                if (list.Count == 0)
                {
                    prev = 0.0;
                    next = 1.0;
                }
                else if (i == 0)
                {
                    prev = 0.0;
                    next = list[0];
                }
                else if (i == list.Count)
                {
                    prev = list[i - 1];
                    next = 1.0;
                }
                else
                {
                    prev = list[i - 1];
                    next = list[i];
                }

                if (getPrev)
                {
                    if (i == 0 && position == 0.0 || position >= prev + alpha && position < next + alpha || i == list.Count && position == 1.0)
                    {
                        return prev;
                    }
                }
                else
                {
                    if (i == 0 && position == 0.0 || position >= prev && position < next || i == list.Count && position == 1.0)
                    {
                        return next;
                    }
                }
            }

            // 何処かに引っかかるのでここには来ないはず
            return 0.0;
        }
    }
}
