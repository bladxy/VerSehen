using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerSehen.MVVM.Model;
using Action = VerSehen.MVVM.Model.Action;

namespace VerSehen.Core
{
    public class QTableEntry
    {
        public State State { get; set; }
        public Dictionary<Action, double> Actions { get; set; }
    }
}
