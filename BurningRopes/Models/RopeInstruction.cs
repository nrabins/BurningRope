using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BurningRopes.Models
{
    public class RopeInstruction
    {
        public int ropeId;
        public double t1;
        public double t2;
        public double endTime;

        public RopeInstruction(int ropeId, double t1, double t2, double endTime)
        {
            this.ropeId = ropeId;
            this.t1 = t1;
            this.t2 = t2;
            this.endTime = endTime;
        }
    }
}
