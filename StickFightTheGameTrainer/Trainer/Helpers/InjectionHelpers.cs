﻿using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StickFightTheGameTrainer.Trainer.Helpers
{
    public class InjectionHelpers
    {
        private readonly Common.ILogger _logger;

        public InjectionHelpers(Common.ILogger logger)
        {
            _logger = logger;
        }

        public void Save(ModuleDefMD targetModule, bool overwrite)
        {
            var targetLocation = targetModule.Location;
            var patchedLocation = targetLocation.Replace(".dll", "_patched.dll");

            targetModule.Write(patchedLocation);

            if (overwrite)
            {
                targetModule.Dispose();
                File.Copy(patchedLocation, targetLocation, true);
            }
        }

        public bool AddTypeToModule(TypeDef typeDef, ModuleDefMD moduleDefMdTarget, bool overwrite = false, bool detachFromSourceModuleTypes = true)
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

        public async Task ClearMethodBody(ModuleDefMD moduleDef, string typeDefName, string methodDefName)
        {
            var typeDef = moduleDef.Find(typeDefName, true);

            if (typeDef == null)
            {
                await _logger.Log($"Could not clear method body: Type def '{typeDefName}' could not be located");
                return;
            }

            var methodDef = typeDef.FindMethod(methodDefName);

            if (methodDef == null)
            {
                await _logger.Log($"Could not clear method body: Method def '{methodDefName}' could not be located");
                return;
            }

            if (methodDef.Body == null)
            {
                methodDef.Body = new CilBody();
            }

            methodDef.Body.Instructions.Clear();
            methodDef.Body.Instructions.Add(new Instruction(OpCodes.Ret));
        }

        public bool AddMethodToType(MethodDef methodDef, TypeDef typeDefTarget, bool overwrite = false, bool detachFromSourceModuleTypes = true)
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

        public async Task SetFieldAccessModifier(ModuleDefMD module, string typeDefName, string fieldName, FieldAttributes fieldAttribute, bool isReflectionName = true)
        {
            var typeDef = module.Find(typeDefName, isReflectionName);

            if (typeDef == null)
            {
                await _logger.Log($"Set Field Access Modifier: Type def not found: {typeDefName}");
                return;
            }

            var fieldDef = typeDef.FindField(fieldName);

            if (fieldDef == null)
            {
                await _logger.Log($"Set Field Access Modifier: Field def not found: {fieldName}");
                return;
            }

            fieldDef.Access = fieldAttribute;
        }

        public async Task SetMethodAccessModifier(ModuleDefMD module, string typeDefName, string methodName, MethodAttributes methodAttribute)
        {
            var typeDef = module.Find(typeDefName, true);

            if (typeDef == null)
            {
                await _logger.Log($"Set Method Access Modifier: Type def not found: {typeDefName}");
                return;
            }

            var methodDef = typeDef.FindMethod(methodName);

            if (methodDef == null)
            {
                await _logger.Log($"Set Method Access Modifier: Method def not found: {methodName}");
                return;
            }

            methodDef.Access = methodAttribute;
        }

        public async Task AddField(ModuleDefMD module, string typeDefName, string fieldName, TypeSig type, FieldAttributes fieldAttributeLeft, FieldAttributes fieldAttributeRight)
        {
            var typeDef = module.Find(typeDefName, true);

            if (typeDef == null)
            {
                await _logger.Log($"Add Field: Type def not found: {typeDefName}");
                return;
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
                await _logger.Log($"Add Field: Field name '{fieldName}' already exists in type '{typeDefName}'");
            }
        }

        public async Task<FieldDef> AddField(ModuleDefMD module, string typeDefName, string fieldName, TypeSig type, FieldAttributes fieldAttributeLeft)
        {
            var typeDef = module.Find(typeDefName, true);

            if (typeDef == null)
            {
                await _logger.Log($"Add Field: Type def not found: {typeDefName}");
                return null;
            }

            var fieldDef = new FieldDefUser(fieldName,
                new FieldSig(type),
                fieldAttributeLeft);

            var existingTypeDef = typeDef.Fields.FirstOrDefault(field => field.Name == fieldName);
            if (existingTypeDef != null)
            {
                await _logger.Log($"Add Field: Field name '{fieldName}' already exists in type '{typeDefName}'");
                return existingTypeDef;
            }

            typeDef.Fields.Add(fieldDef);

            return await Task.FromResult(fieldDef);
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
    }
}