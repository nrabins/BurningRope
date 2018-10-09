using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BurningRopes.Models;

namespace BurningRopes
{
    public abstract class RopeSolver
    {
        protected readonly int NumRopes;
        protected readonly double RopeBurnTime;

        public RopeSolver(int numRopes = 3, double ropeBurnTime = 60)
        {
            this.NumRopes = numRopes;
            this.RopeBurnTime = ropeBurnTime;
        }

        public abstract SortedList<double, List<RopeInstruction>> Solve();
    }
}
