using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;

namespace StickFightTheGameTrainer.Trainer.Helpers
{
    /// <summary>
    /// Compare instruction opcodes
    /// </summary>
    public class OpCodeInstructionComparer : IEqualityComparer<Instruction>
    {
        public bool Equals(Instruction x, Instruction y)
        {
            // Note: Can get false positives on small signatures.
            if (x != null && y != null)
            {
                return x.OpCode == y.OpCode;
            }

            return true;
        }

        public int GetHashCode(Instruction obj)
        {
            throw new NotImplementedException();
        }
    }
}
