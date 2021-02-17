using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StickFightTheGameTrainer.Trainer.Helpers
{
    public static class InjectionHelpers
    {
        public static async Task<byte[]> Save(ModuleDefMD targetModule, bool writeToFile)
        {
            var targetLocation = targetModule.Location;

            using (var ms = new MemoryStream())
            {
                targetModule.Write(ms);
                targetModule.Dispose();

                ms.Seek(0, SeekOrigin.Begin);
                var data = ms.ToArray();
                if (writeToFile)
                {
                    using (var fs = new FileStream(targetLocation, FileMode.Create))
                    {
                        await fs.WriteAsync(data, 0, data.Length);
                        fs.Flush();
                    }
                }

                return data;
            }
        }

        public static bool AddTypeToModule(TypeDef typeDef, ModuleDefMD moduleDefMdTarget, bool overwrite = false, bool detachFromSourceModuleTypes = true)
        {
            if (typeDef == null || moduleDefMdTarget == null)
            {
                return false;
            }

            // Check if type is already present
            var existingTypeDef = moduleDefMdTarget.Types.FirstOrDefault(type => type.FullName == typeDef.FullName);
            if (existingTypeDef != null)
            {
                if (!overwrite)
                {
                    return true;
                }

                moduleDefMdTarget.Types.Remove(existingTypeDef);
            }

            // Detach from associated module before injecting into target
            if (detachFromSourceModuleTypes)
            {
                typeDef.Module?.Types?.Remove(typeDef);
            }

            moduleDefMdTarget.Types.Add(typeDef);

            return true;
        }

        public static bool RemoveTypeFromModule(string typeName, ModuleDefMD moduleDefMdTarget)
        {
            if (typeName == null || moduleDefMdTarget == null)
            {
                return false;
            }

            var typeDefMatch = moduleDefMdTarget.Find(typeName, false);

            if (typeDefMatch != null)
            {
                moduleDefMdTarget.Types.Remove(typeDefMatch);
                return true;
            }

            return false;
        }

        public static bool ClearMethodBody(ModuleDefMD moduleDef, string typeDefName, string methodDefName)
        {
            var typeDef = moduleDef.Find(typeDefName, true);

            if (typeDef == null)
            {
                return false;
            }

            var methodDef = typeDef.FindMethod(methodDefName);

            if (methodDef == null)
            {
                return false;
            }

            if (methodDef.Body == null)
            {
                methodDef.Body = new CilBody();
            }

            methodDef.Body.Instructions.Clear();
            methodDef.Body.Instructions.Add(new Instruction(OpCodes.Ret));

            return true;
        }

        public static bool AddMethodToType(MethodDef methodDef, TypeDef typeDefTarget, bool overwrite = false, bool detachFromSourceModuleTypes = true)
        {
            if (methodDef == null || typeDefTarget == null)
            {
                return false;
            }

            // Check if method is already present
            var existingMethodDef = typeDefTarget.Methods.FirstOrDefault(type => type.FullName == methodDef.FullName);
            if (existingMethodDef != null)
            {
                if (!overwrite)
                {
                    return true;
                }

                typeDefTarget.Methods.Remove(existingMethodDef);
            }

            // Detach from associated type before injecting into target
            if (detachFromSourceModuleTypes)
            {
                methodDef.DeclaringType = null;
            }

            typeDefTarget.Methods.Add(methodDef);

            return true;
        }

        public static bool SetFieldAccessModifier(ModuleDefMD module, string typeDefName, string fieldName, FieldAttributes fieldAttribute, bool isReflectionName = true)
        {
            var typeDef = module.Find(typeDefName, isReflectionName);

            if (typeDef == null)
            {
                return false;
            }

            var fieldDef = typeDef.FindField(fieldName);

            if (fieldDef == null)
            {
                return false;
            }

            fieldDef.Access = fieldAttribute;

            return true;
        }

        public static bool SetMethodAccessModifier(ModuleDefMD module, string typeDefName, string methodName, MethodAttributes methodAttribute)
        {
            var typeDef = module.Find(typeDefName, true);

            if (typeDef == null)
            {
                return false;
            }

            var methodDef = typeDef.FindMethod(methodName);

            if (methodDef == null)
            {
                return false;
            }

            methodDef.Access = methodAttribute;

            return true;
        }

        public static bool AddField(ModuleDefMD module, string typeDefName, string fieldName, TypeSig type, FieldAttributes fieldAttributeLeft, FieldAttributes fieldAttributeRight)
        {
            var typeDef = module.Find(typeDefName, true);

            if (typeDef == null)
            {
                return false;
            }

            var fieldDef = new FieldDefUser(fieldName,
                new FieldSig(type),
                fieldAttributeLeft | fieldAttributeRight);

            if (typeDef.Fields.Any(field => field.Name == fieldName))
            {
                typeDef.Fields.Add(fieldDef);
            }
            else
            {
                return false;
            }

            return true;
        }

        public static FieldDef AddField(ModuleDefMD module, string typeDefName, string fieldName, TypeSig type, FieldAttributes fieldAttributeLeft)
        {
            var typeDef = module.Find(typeDefName, true);

            if (typeDef == null)
            {
                return null;
            }

            var fieldDef = new FieldDefUser(fieldName,
                new FieldSig(type),
                fieldAttributeLeft);

            var existingTypeDef = typeDef.Fields.FirstOrDefault(field => field.Name == fieldName);
            if (existingTypeDef != null)
            {
                return existingTypeDef;
            }

            typeDef.Fields.Add(fieldDef);

            return fieldDef;
        }

        public static PropertyDef AddProperty(ModuleDefMD module, string typeDefName, string propertyName, PropertySig type, PropertyAttributes propertyAttributeLeft)
        {
            var typeDef = module.Find(typeDefName, true);

            if (typeDef == null)
            {
                return null;
            }

            var propertyDef = new PropertyDefUser(propertyName, type, propertyAttributeLeft);

            var existingTypeDef = typeDef.Properties.FirstOrDefault(property => property.Name == propertyName);
            if (existingTypeDef != null)
            {
                return existingTypeDef;
            }

            typeDef.Properties.Add(propertyDef);

            return propertyDef;
        }

        /// <summary>
        /// Find all references to a TypeDef and replace them with another TypeDef.
        /// Note that field and method references from the source TypeDef will also need to be replaced serparately.
        /// </summary>
        public static void ReplaceAllTypeDefReferences(TypeDef sourceTypeDef, TypeDef destinationTypeDef, ModuleDefMD moduleDefMd)
        {
            var types = moduleDefMd.GetTypes();

            var sourceTypeSig = sourceTypeDef.ToTypeSig();
            var destinationTypeSig = destinationTypeDef.ToTypeSig();

            var sigComparer = new SigComparer();

            foreach (var type in types)
            {
                // Fields
                foreach (var field in type.Fields)
                {
                    if (sigComparer.Equals(field.FieldType, sourceTypeSig))
                    {
                        field.FieldType = destinationTypeSig;
                    }
                }

                // Methods
                foreach (var method in type.Methods)
                {
                    if (method.Body == null)
                    {
                        continue;
                    }

                    foreach (var instruction in method.Body.Instructions)
                    {
                        if (instruction.Operand != null)
                        {
                            if (instruction.Operand is MethodSpec methodSpec && methodSpec.GenericInstMethodSig != null)
                            {
                                // Replace generic parameter types
                                for (var i = methodSpec.GenericInstMethodSig.GenericArguments.Count - 1; i >= 0; i--)
                                {
                                    var genericArgument = methodSpec.GenericInstMethodSig.GenericArguments[i];
                                    var genericArgumentTypeSpec = genericArgument as TypeSig;
                                    if (sigComparer.Equals(genericArgumentTypeSpec, sourceTypeSig))
                                    {
                                        methodSpec.GenericInstMethodSig.GenericArguments.RemoveAt(i);
                                        methodSpec.GenericInstMethodSig.GenericArguments.Insert(i, destinationTypeSig);
                                    }
                                }

                                // Todo: Parameters
                                // Todo: Return types.
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Wrap a try/catch block around the entire body of a method.
        /// </summary>
        /// <param name="body">Target method CilBody that will be wrapped with a try/catch block</param>
        /// <param name="catchHandler">Optional collection of instructions to place in the exception handler.</param>
        /// <param name="catchType">Optional type of exception (defaults to System.Object)</param>
        public static void WrapTryCatchHandler(CilBody body, List<Instruction> catchHandler = null, ITypeDefOrRef catchType = null)
        {
            var finalInstructionIndex = body.Instructions.Count - 1;
            var ret = new Instruction(OpCodes.Ret);

            body.Instructions.Add(new Instruction(OpCodes.Leave_S, ret));
            body.Instructions.Add(ret);

            body.ExceptionHandlers.Add(new ExceptionHandler
            {
                TryStart = body.Instructions[0],
                TryEnd = body.Instructions[finalInstructionIndex],
                HandlerStart = body.Instructions[finalInstructionIndex],
                HandlerEnd = ret,
                HandlerType = ExceptionHandlerType.Catch,
                CatchType = ModuleDefMD.Load(typeof(object).Module).CorLibTypes.Object.TypeDef
                //CatchType = ModuleDefMD.Load(typeof(object).Module).Find("object", false)
                //CatchType = new ModuleDefUser().CorLibTypes.Object.TypeDef
            });
        }

        public static List<Instruction> FetchInstructionsBySignature(IList<Instruction> instructions, IList<OpCode> signatureOpcodes)
        {
            for (var i = 0; i <= instructions.Count - signatureOpcodes.Count; i++)
            {
                var subset = instructions.Skip(i).Take(signatureOpcodes.Count).ToArray();
                if (subset.Select(instruction => instruction.OpCode).SequenceEqual(signatureOpcodes))
                {
                    return subset.ToList();
                }
            }

            return null;
        }

        public static List<Instruction> FetchInstructionsBySigComparerSignature(IList<Instruction> instructions, IList<Instruction> signatureInstructions)
        {
            for (var i = 0; i <= instructions.Count - signatureInstructions.Count; i++)
            {
                var subset = instructions.Skip(i).Take(signatureInstructions.Count).ToArray();

                if (subset.SequenceEqual(signatureInstructions, new SignatureComparer()))
                {
                    return subset.ToList();
                }
            }

            return null;
        }

        public static List<Instruction> FetchInstructionsBySignature(IList<Instruction> instructions, IList<Instruction> signatureInstructions, bool strict = true)
        {
            for (var i = 0; i <= instructions.Count - signatureInstructions.Count; i++)
            {
                var subset = instructions.Skip(i).Take(signatureInstructions.Count).ToArray();

                if (strict)
                {
                    if (subset.SequenceEqual(signatureInstructions, new InstructionComparer()))
                    {
                        return subset.ToList();
                    }
                }
                else
                {
                    if (subset.SequenceEqual(signatureInstructions, new FlexibleInstructionComparer()))
                    {
                        return subset.ToList();
                    }
                }
            }

            return null;
        }

        public static List<Instruction> FetchInstructionsByOpcodeSignature(IList<Instruction> instructions, IList<Instruction> signatureInstructions)
        {
            for (var i = 0; i <= instructions.Count - signatureInstructions.Count; i++)
            {
                var subset = instructions.Skip(i).Take(signatureInstructions.Count).ToArray();

                if (subset.SequenceEqual(signatureInstructions, new OpCodeInstructionComparer()))
                {
                    return subset.ToList();
                }
            }

            return null;
        }
    }
}
