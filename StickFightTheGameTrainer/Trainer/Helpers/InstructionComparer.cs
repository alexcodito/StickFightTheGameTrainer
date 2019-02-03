using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;

namespace StickFightTheGameTrainer.Trainer.Helpers
{
    public class InstructionComparer : IEqualityComparer<Instruction>
    {
        public bool Equals(Instruction x, Instruction y)
        {
            return x != null && y != null && x.OpCode == y.OpCode && x.Operand == y.Operand;
        }

        public int GetHashCode(Instruction obj)
        {
            throw new NotImplementedException();
        }
    }
}
