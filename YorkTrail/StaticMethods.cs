/*
    YorkTrail
    Copyright (C) 2021 theta

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
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
