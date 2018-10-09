using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BurningRopes.Models
{
    public enum WhichEnd
    {
        First,
        Second,
        Both
    }

    public class RopeAction
    {
        public int ropeId;
        public WhichEnd whichEnd;

        public RopeAction(int ropeId, WhichEnd whichEnd)
        {
            this.ropeId = ropeId;
            this.whichEnd = whichEnd;
        }

        public string ToHumanReadableString()
        {
            switch (whichEnd)
            {
                case WhichEnd.First:
                    return $"light one end of rope {ropeId}";
                case WhichEnd.Second:
                    return $"light the other end of rope {ropeId}";
                case WhichEnd.Both:
                default:
                    return $"light both ends of rope {ropeId}";
            }
        }
    }
}
