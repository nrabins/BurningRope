using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BurningRopes.Models;

namespace BurningRopes.Solvers
{
    public class BreadthFirstSearchSolver : RopeSolver
    {
        public BreadthFirstSearchSolver(int numRopes = 3, double ropeBurnTime = 60) : base(numRopes, ropeBurnTime)
        {
        }

        public override SortedList<double, List<RopeInstruction>> Solve()
        {
            var frontier = new Queue<RopeState>();
            frontier.Enqueue(new RopeState
            {
                RopesRemaining = NumRopes,
                AvailablePoints = new List<double> { 0 },
                MostPreciseTime = 0,
                Instructions = new List<RopeInstruction>()
            });

            var timesAndInstructions = new SortedList<double, List<RopeInstruction>>();

            // While we have stuff to test, keep testing
            while (frontier.Count > 0)
            {
                var testState = frontier.Dequeue();
                
                // Test the current state. If this is the first instance of a reachable time OR if we used fewer ropes to get this time, put it in our list
                if (!timesAndInstructions.ContainsKey(testState.MostPreciseTime) || testState.Instructions.Count < timesAndInstructions[testState.MostPreciseTime].Count)
                {
                    timesAndInstructions[testState.MostPreciseTime] = testState.Instructions;
                }

                if (testState.RopesRemaining == 0)
                    continue;

                for (int t1Index = 0; t1Index < testState.AvailablePoints.Count; t1Index++)
                {
                    var t1 = testState.AvailablePoints[t1Index];

                    // We add a point one rope burn time away from the starting point to simulate only lighting one end of the rope
                    var availablePointsAndDummyEnd = new List<double>(testState.AvailablePoints);
                    availablePointsAndDummyEnd.Add(t1 + RopeBurnTime);

                    for (int t2Index = t1Index; t2Index < availablePointsAndDummyEnd.Count; t2Index++)
                    {
                        var t2 = availablePointsAndDummyEnd[t2Index];

                        // See exact math in DepthFirstSearchSolver.cs
                        var endTime = (t1 + t2 + RopeBurnTime) / 2;

                        if (testState.AvailablePoints.Contains(endTime))
                            continue; // We gained no information, prune this branch

                        var newAvailablePoints = new List<double>(testState.AvailablePoints);
                        newAvailablePoints.Add(endTime);

                        var newInstructions = new List<RopeInstruction>(testState.Instructions);
                        newInstructions.Add(new RopeInstruction(NumRopes - testState.RopesRemaining + 1, t1, t2, endTime));

                        if (timesAndInstructions.ContainsKey(endTime) &&
                            newInstructions.Count >= timesAndInstructions[endTime].Count)
                            continue; // We found a less efficient way to find a time, prune this branch

                        var newTestState = new RopeState()
                        {
                            AvailablePoints = newAvailablePoints,
                            RopesRemaining = testState.RopesRemaining - 1,
                            MostPreciseTime = endTime,
                            Instructions = newInstructions
                        };

                        frontier.Enqueue(newTestState);
                    }
                }
                
            }


            return timesAndInstructions;
        }
    }

    public class RopeState
    {
        public int RopesRemaining { get; set; }
        public List<double> AvailablePoints { get; set; }
        public List<RopeInstruction> Instructions { get; set; }
        public double MostPreciseTime { get; set; }
    }
}
