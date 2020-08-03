using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;

namespace StickFightTheGameTrainer.Trainer.Helpers
{
    public class SignatureComparer : IEqualityComparer<Instruction>
    {
        public bool Equals(Instruction x, Instruction y)
        {
            var sigComparer = new SigComparer();

            if (x != null && y != null && x.OpCode == y.OpCode)
            {
                if(x.Operand == null && y.Operand == null)
                {
                    return true;
                }

                if (x.Operand is FieldDef xFieldDef && y.Operand is FieldDef yFieldDef)
                {
                    return sigComparer.Equals(xFieldDef, yFieldDef);
                }

                if (x.Operand is MemberRef xMemberRef && y.Operand is MemberRef yMemberRef)
                {
                    return sigComparer.Equals(xMemberRef, yMemberRef);
                }

                if (x.Operand is IField xIField && y.Operand is IField yIField)
                {
                    return sigComparer.Equals(xIField, yIField);
                }

                if (x.Operand is MethodSpec xMethodSpec && y.Operand is MethodSpec yMethodSpec)
                {
                    return sigComparer.Equals(xMethodSpec, yMethodSpec);
                }

                if (x.Operand is MethodDef xMethodDef && y.Operand is MethodDef yMethodDef)
                {
                    return sigComparer.Equals(xMethodDef, yMethodDef);
                }

                if (x.Operand is IMethod xIMethod && y.Operand is IMethod yIMethod)
                {
                    return sigComparer.Equals(xIMethod, yIMethod);
                }

                if (x.Operand is TypeSpec xTypeSpec && y.Operand is TypeSpec yTypeSpec)
                {
                    return sigComparer.Equals(xTypeSpec, yTypeSpec);
                }

                if (x.Operand is TypeSig xTypeSig && y.Operand is TypeSig yTypeSig)
                {
                    return sigComparer.Equals(xTypeSig, yTypeSig);
                }

                if (x.Operand is MethodSig xMethodSig && y.Operand is MethodSig yMethodSig)
                {
                    return sigComparer.Equals(xMethodSig, yMethodSig);
                }

                if (x.Operand is TypeRef xTypeRef && y.Operand is TypeRef yTypeRef)
                {
                    return sigComparer.Equals(xTypeRef, yTypeRef);
                }

                if (x.Operand is TypeDef xTypeDef && y.Operand is TypeDef yTypeDef)
                {
                    return sigComparer.Equals(xTypeDef, yTypeDef);
                }

                if (x.Operand is FieldSig xFieldSig && y.Operand is FieldSig yFieldSig)
                {
                    return sigComparer.Equals(xFieldSig, yFieldSig);
                }

                if (x.Operand is ITypeDefOrRef xITypeDefOrRef && y.Operand is ITypeDefOrRef yITypeDefOrRef)
                {
                    return sigComparer.Equals(xITypeDefOrRef, yITypeDefOrRef);
                }

                if (x.Operand is GenericInstMethodSig xGenericInstMethodSig && y.Operand is GenericInstMethodSig yGenericInstMethodSig)
                {
                    return sigComparer.Equals(xGenericInstMethodSig, yGenericInstMethodSig);
                }

                if (x.Operand is IMemberRef xIMemberRef && y.Operand is IMemberRef yIMemberRef)
                {
                    return sigComparer.Equals(xIMemberRef, yIMemberRef);
                }

                if (x.Operand is IType xIType && y.Operand is IType yIType)
                {
                    return sigComparer.Equals(xIType, yIType);
                }

                if (x.Operand is EventDef xEventDef && y.Operand is EventDef yEventDef)
                {
                    return sigComparer.Equals(xEventDef, yEventDef);
                }

                if (x.Operand is PropertyDef xPropertyDef && y.Operand is PropertyDef yPropertyDef)
                {
                    return sigComparer.Equals(xPropertyDef, yPropertyDef);
                }

                if (x.Operand is ExportedType xExportedType && y.Operand is ExportedType yExportedType)
                {
                    return sigComparer.Equals(xExportedType, yExportedType);
                }

                if (x.Operand is LocalSig xLocalSig && y.Operand is LocalSig yLocalSig)
                {
                    return sigComparer.Equals(xLocalSig, yLocalSig);
                }

                if (x.Operand is IList<TypeSig> xIList && y.Operand is IList<TypeSig> yIList)
                {
                    return sigComparer.Equals(xIList, yIList);
                }

                if (x.Operand is MethodBaseSig xMethodBaseSig && y.Operand is MethodBaseSig yMethodBaseSig)
                {
                    return sigComparer.Equals(xMethodBaseSig, yMethodBaseSig);
                }

                if (x.Operand is CallingConventionSig xCallingConventionSig && y.Operand is CallingConventionSig yCallingConventionSig)
                {
                    return sigComparer.Equals(xCallingConventionSig, yCallingConventionSig);
                }

                return x.Operand == y.Operand;
            }

            return false;
        }

        public int GetHashCode(Instruction obj)
        {
            throw new NotImplementedException();
        }
    }
}
