// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.PowerFx.Core.Types;
using Microsoft.PowerFx.Types;
using Xunit;

namespace Microsoft.PowerFx.Tests
{
    public class CollectFunctionWithLazyTypesTests
    {
        public class CustomTypeRecordType : RecordType
        {
            public readonly string TypeName;
            private readonly IDictionary<string, FormulaType> _fieldTypes = new Dictionary<string, FormulaType>();

            public CustomTypeRecordType(string typeName)
            {
                TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            }

            public override IEnumerable<string> FieldNames => _fieldTypes.Select(field => field.Key);

            public void SetTypeProperties(IDictionary<string, FormulaType> typeProperties)
            {
                foreach (var kvp in typeProperties)
                {
                    _fieldTypes[kvp.Key] = kvp.Value;
                    Add(kvp.Key, kvp.Value);
                }
            }

            #region Method overrides

            public override RecordType Add(NamedFormulaType field)
            {
                _fieldTypes[field.Name] = field.Type;
                return this;
            }

            public override bool TryGetFieldType(string name, out FormulaType type)
            {
                return _fieldTypes.TryGetValue(name, out type);
            }

            public override bool Equals(object other)
            {
                return (other is CustomTypeRecordType otherType) && TypeName == otherType.TypeName;
            }

            public override int GetHashCode()
            {
                return TypeName.GetHashCode();
            }

            #endregion
        }

        [Fact]
        public void CheckCollectFunctionWithLazyTypesTest()
        {
            var engine = new RecalcEngine();
            engine.Config.SymbolTable.EnableMutationFunctions();
            engine.Config.EnableSetFunction();
            var o = new ParserOptions() { AllowsSideEffects = true };

            engine.Config.SymbolTable.AddVariable("robinCustomTypeTable", new CustomTypeRecordType("typeName").ToTable(), true);
            engine.Config.SymbolTable.AddVariable("robinCustomType", new CustomTypeRecordType("typeName"), true);

            var result = engine.Check("Collect(robinCustomTypeTable, robinCustomType)", o); // 20

            Assert.True(result.IsSuccess);
        }
    }
}
