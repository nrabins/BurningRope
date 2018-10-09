using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BurningRopes.Models;

namespace BurningRopes.Solvers
{
    class DepthFirstSearchSolver : RopeSolver
    {
        public DepthFirstSearchSolver(int numRopes, double ropeBurnTime) 
            : base(numRopes, ropeBurnTime)
        {
        }

        public override SortedList<double, List<RopeInstruction>> Solve()
        {
            var allKnownTimesWithInstructions = new SortedList<double, List<RopeInstruction>>
            {
                {0, new List<RopeInstruction>()}
            };

            CalculateTimesAndInstructions(ref allKnownTimesWithInstructions, new List<double> { 0 }, new List<RopeInstruction>(), 0);
            return allKnownTimesWithInstructions;
        }

        /// <summary>
        /// Accumulates all possible times and rope burning instructions given an arbitrary state
        /// </summary>
        /// <param name="allKnownTimesWithInstructions">Accumulates all times with their requisite rope burning instructions</param>
        /// <param name="currentKnownTimes">Times available given the instructions to this point</param>
        /// <param name="instructionsSoFar">Instructions to this point</param>
        /// <param name="ropesRemaining">How many ropes remain to be burned</param>
        public void CalculateTimesAndInstructions(
            ref SortedList<double, List<RopeInstruction>> allKnownTimesWithInstructions,
            List<double> currentKnownTimes,
            List<RopeInstruction> instructionsSoFar,
            int ropesUsed)
        {
            if (ropesUsed >= NumRopes)
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
                currentKnownTimesAndEnd.Add(t1 + RopeBurnTime);
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
                    var endTime = (t1 + t2 + RopeBurnTime) / 2;

                    AccumulateAndRecurse(ref allKnownTimesWithInstructions, currentKnownTimes, endTime, instructionsSoFar, t1, t2, ropesUsed);
                }
            }
        }

        private void AccumulateAndRecurse(ref SortedList<double, List<RopeInstruction>> allKnownTimes, List<double> currentKnownTimes, double endTime, List<RopeInstruction> instructionsSoFar, double t1, double t2, int ropesUsed)
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
    }
}
