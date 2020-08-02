using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;

namespace StickFightTheGameTrainer.Trainer.Helpers
{
    /// <summary>
    /// Less strict comparison
    /// </summary>
    public class FlexibleInstructionComparer : IEqualityComparer<Instruction>
    {
        public bool Equals(Instruction x, Instruction y)
        {
            // Opcodes for which operands are not possible to calculate
            // - E.g. Can't calculate branch condition offset, so consider Brtrue instructions equal regardless of operand (i.e. the 
            // signature's Brtrue instruction operand would be null as the jump address is irrelevant)
            // Note: Can get false positives on small signatures.
            var flexibleOpcodes = new List<OpCode> { 
                OpCodes.Br, 
                OpCodes.Brtrue, 
                OpCodes.Brtrue_S, 
                OpCodes.Brfalse, 
                OpCodes.Brfalse_S, 
                OpCodes.Br_S 
            };

            if (x != null && y != null)
            {
                if (x.OpCode == y.OpCode)
                {
                    if (flexibleOpcodes.Contains(x.OpCode))
                    {
                        return true;
                    }

                    return x.OpCode == y.OpCode && x.Operand == y.Operand;
                }

                return false;
            }

            return true;
        }

        public int GetHashCode(Instruction obj)
        {
            throw new NotImplementedException();
        }
    }
}
