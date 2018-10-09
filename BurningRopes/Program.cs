using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace BurningRopes
{
    class Program
    {
        private static readonly double ROPE_BURN_TIME = 60;
        private static readonly int NUM_ROPES = 3;

        public static void Main(string[] args)
        {
            var allKnownTimesWithInstructions = new SortedList<double, List<RopeInstruction>>
            {
                {0, new List<RopeInstruction>()}
            };

            CalculateTimesAndInstructions(ref allKnownTimesWithInstructions, new List<double>{ 0 }, new List<RopeInstruction>(), 0);

            foreach (var knownTime in allKnownTimesWithInstructions)
            {
                var humanReadableInstructions = GetHumanReadableInstructions(knownTime.Key, knownTime.Value);
                foreach (var humanReadableInstruction in humanReadableInstructions)
                {
                    Console.WriteLine(humanReadableInstruction);
                }

                Console.WriteLine();
            }

            Console.ReadKey();

        }


        /// <summary>
        /// Accumulates all possible times and rope burning instructions given an arbitrary state
        /// </summary>
        /// <param name="allKnownTimesWithInstructions">Accumulates all times with their requisite rope burning instructions</param>
        /// <param name="currentKnownTimes">Times available given the instructions to this point</param>
        /// <param name="instructionsSoFar">Instructions to this point</param>
        /// <param name="ropesRemaining">How many ropes remain to be burned</param>
        public static void CalculateTimesAndInstructions(
            ref SortedList<double, List<RopeInstruction>> allKnownTimesWithInstructions, 
            List<double> currentKnownTimes, 
            List<RopeInstruction> instructionsSoFar, 
            int ropesUsed)
        {
            if (ropesUsed >= NUM_ROPES)
                return;

            // Walk the available times start to finish. We can be a little more efficient by avoiding duplicate states
            // (by starting the right rope at or after the left rope).
            for (var firstTimeIndex = 0; firstTimeIndex < currentKnownTimes.Count; firstTimeIndex++)
            {
                var t1 = currentKnownTimes.ElementAt(firstTimeIndex);

                // Instead of handling an edge case after our 2-dimensional traversal,
                // we imitate not lighting the second end of the rope by lighting it
                // as the rope finishes burning from the first end (t1 + RL)
                var currentKnownTimesAndEnd = new List<double>(currentKnownTimes);
                currentKnownTimesAndEnd.Add(t1 + ROPE_BURN_TIME);
                for (int secondTimeIndex = firstTimeIndex; secondTimeIndex < currentKnownTimesAndEnd.Count; secondTimeIndex++)
                {
                    var t2 = currentKnownTimesAndEnd.ElementAt(secondTimeIndex);

                    // Math Time!
                    // The formula for determining when a rope will finish burning
                    // given rope lighting times t1 and t2 and rope burn time RBT is...

                    // end time = start time + time spent burning one end of rope + time spent burning rope from both ends
                    //          = t1 + (t2 - t1 + (RBT + t1 - t2) / 2;
                    //          = t2 + (RBT + t1 - t2) / 2;
                    //          = (2*t2 + RBT + t1 - t2) / 2;
                    //          = (t2 + RBT + t1) / 2;
                    var endTime = (t1 + t2 + ROPE_BURN_TIME) / 2;

                    AccumulateAndRecurse(ref allKnownTimesWithInstructions, currentKnownTimes, endTime, instructionsSoFar, t1, t2, ropesUsed);
                }
            }
        }

        private static void AccumulateAndRecurse(ref SortedList<double, List<RopeInstruction>> allKnownTimes, List<double> currentKnownTimes, double endTime, List<RopeInstruction> instructionsSoFar, double t1, double t2, int ropesUsed)
        {
            if (currentKnownTimes.Contains(endTime))
                return;

            // If our times are out of order, then our path to this point is functionally a duplicate of an existing path.
            // For example: the first rope is t1=0, t1=60, endTime=60.
            // The second rope recurses from that point and attempts t1=0, t2=0, endTime = 30.
            // This is equivalent to the case where they are swapped; it doesn't matter which rope is which. 
            // We are assured of already having this because we generate the ropes in ascending order.
            // There is probably a saner way to curtail these duplicates, but I don't want to pass in a history if possible.
            // For now, we just abandon this branch if we get something in reverse order.
            if (t1 > t2)
                return;

            var knownTimesClone = new List<double>(currentKnownTimes);
            knownTimesClone.Add(endTime);

            var instructionsClone = new List<RopeInstruction>(instructionsSoFar);
            instructionsClone.Add(new RopeInstruction(ropesUsed + 1, t1, t2, endTime));

            if (!allKnownTimes.ContainsKey(endTime))
            {
                allKnownTimes.Add(endTime, instructionsClone);
            }

            CalculateTimesAndInstructions(ref allKnownTimes, knownTimesClone, instructionsClone, ropesUsed + 1);
        }


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
        private static List<string> GetHumanReadableInstructions(double time, List<RopeInstruction> ropeInstructions)
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
            lines.Add($"{time} minutes");

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

    }

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
                for (int i = 0; i < actions.Count-1; i++)
                {
                    builder.Append(actions[i].ToHumanReadableString()).Append(", ");
                }

                builder.Append("and ").Append(actions[actions.Count - 1].ToHumanReadableString());
                actionStr = builder.ToString();
            }

            return $"{timeStr}: {startStr}, {actionStr}";
        }
    }

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