using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BurningRopes.Models
{
    public class TimeInstruction
    {
        public double time;
        public int? endRopeId;
        public List<RopeAction> actions;

        public TimeInstruction(double time)
        {
            this.time = time;
            this.actions = new List<RopeAction>();
        }

        public string ToHumanReadableString()
        {
            var timeStr = $"{time} min".PadRight(8);

            var startStr = endRopeId == null
                ? "To start"
                : $"When rope {endRopeId} has finished";

            var actionStr = "";
            if (actions.Count == 0)
            {
                actionStr = "you've reached your time!";
            }
            else if (actions.Count == 1)
            {
                actionStr = $"{actions[0].ToHumanReadableString()}.";
            }
            else if (actions.Count == 2)
            {
                actionStr = $"{actions[0].ToHumanReadableString()} and {actions[1].ToHumanReadableString()}.";
            }
            else
            {
                var builder = new StringBuilder();
                for (int i = 0; i < actions.Count - 1; i++)
                {
                    builder.Append(actions[i].ToHumanReadableString()).Append(", ");
                }

                builder.Append("and ").Append(actions[actions.Count - 1].ToHumanReadableString());
                actionStr = builder.ToString();
            }

            return $"{timeStr}: {startStr}, {actionStr}";
        }
    }

}
