using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BurningRopes.Models;

namespace BurningRopes.Utility
{
    public class FormatUtilities
    {
        /// <summary>
        /// Takes a list of rope instructions and returns a list of human-readable lines in the following format:
        ///
        /// 45 Minutes
        /// 0 min: To start, light rope 1 at both ends, rope 2 at one end.
        /// Minute 30: When rope 1 finishes burning, light rope 2 at the other end.
        /// Minute 45: When rope 2 finishes burning, you've reached 45 minutes.
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <param name="ropeInstructions"></param>
        /// <returns></returns>
        public static List<string> GetHumanReadableInstructions(double time, List<RopeInstruction> ropeInstructions)
        {
            // Generate time instructions for each action time
            var timeInstructions = new SortedList<double, TimeInstruction>();

            foreach (var ropeInstruction in ropeInstructions)
            {
                var t1 = ropeInstruction.t1;
                var t2 = ropeInstruction.t2;
                var endTime = ropeInstruction.endTime;

                // If rope's ends start at same time, record it as a single RopeAction
                if (t1 == t2)
                {
                    if (!timeInstructions.ContainsKey(t1))
                        timeInstructions.Add(t1, new TimeInstruction(t1));
                    timeInstructions[t1].actions.Add(new RopeAction(ropeInstruction.ropeId, WhichEnd.Both));
                }
                else
                {
                    // t1
                    if (!timeInstructions.ContainsKey(t1))
                        timeInstructions.Add(t1, new TimeInstruction(t1));
                    timeInstructions[t1].actions.Add(new RopeAction(ropeInstruction.ropeId, WhichEnd.First));

                    // t2, only if t2 is not equal to the end time (i.e. we were simulating burning a rope all the way through from one end)
                    if (t2 != endTime)
                    {
                        if (!timeInstructions.ContainsKey(t2))
                            timeInstructions.Add(t2, new TimeInstruction(t2));
                        timeInstructions[t2].actions
                            .Add(new RopeAction(ropeInstruction.ropeId, WhichEnd.Second));
                    }
                }

                // endTime
                if (!timeInstructions.ContainsKey(endTime))
                    timeInstructions.Add(endTime, new TimeInstruction(endTime));

                // Don't overwrite the endTime if it's already there; we could get an illogical sequence that way.
                if (timeInstructions[endTime].endRopeId == null)
                    timeInstructions[endTime].endRopeId = ropeInstruction.ropeId;
            }


            var lines = new List<string>();
            lines.Add($"{time} minutes - {ropeInstructions.Count} {(ropeInstructions.Count == 1 ? "rope" : "ropes")}");

            foreach (var timeInstruction in timeInstructions)
            {
                lines.Add(timeInstruction.Value.ToHumanReadableString());
            }

            if (lines.Count == 1)
            {
                lines.Add("You don't need rope to measure 0 minutes...");
            }

            return lines;
        }

        public static string GetSummary(SortedList<double, List<RopeInstruction>> allKnownTimesWithInstructions, int numRopes, double ropeBurnTime)
        {
            return String.Join(Environment.NewLine,
                $"Number of ropes: {numRopes}, Rope burn time: {ropeBurnTime}",
                $"Number of possible times: {allKnownTimesWithInstructions.Count}",
                $"Possible times:",
                String.Join(", ", allKnownTimesWithInstructions.Keys));
        }
    }
}
