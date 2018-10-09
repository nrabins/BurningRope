using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using BurningRopes.Models;
using BurningRopes.Solvers;
using BurningRopes.Utility;

namespace BurningRopes
{
    class Program
    {
        private static readonly double ROPE_BURN_TIME = 60;
        private static readonly int NUM_ROPES = 3;

        public static void Main(string[] args)
        {

            var solver = new DepthFirstSearchSolver(NUM_ROPES, ROPE_BURN_TIME);
            var allKnownTimesWithInstructions = solver.Solve();
            
            foreach (var knownTime in allKnownTimesWithInstructions)
            {
                var humanReadableInstructions = FormatUtilities.GetHumanReadableInstructions(knownTime.Key, knownTime.Value);
                foreach (var humanReadableInstruction in humanReadableInstructions)
                {
                    Console.WriteLine(humanReadableInstruction);
                }

                Console.WriteLine();
            }
            Console.ReadKey();
        }
    }
}