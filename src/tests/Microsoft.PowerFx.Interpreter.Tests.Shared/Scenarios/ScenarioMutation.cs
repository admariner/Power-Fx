﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.PowerFx.Core.IR;
using Microsoft.PowerFx.Core.Tests;
using Microsoft.PowerFx.Types;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.PowerFx.Interpreter.Tests
{
    // Demonstrate mutation example using IUntypedObject
    public class ScenarioMutation : PowerFxTest
    {
        [Fact]
        public void MutabilityTest()
        {
            var config = new PowerFxConfig();
            config.AddFunction(new Assert2Function());
            config.AddFunction(new Set2Function());
            var engine = new RecalcEngine(config);

            var d = new Dictionary<string, FormulaValue>
            {
                ["prop"] = FormulaValue.New(123)
            };

            var obj = MutableObject.New(d);
            engine.UpdateVariable("obj", obj);

            var exprs = new string[]
            {
                "Assert2(obj.prop, 123)",
                "Set2(obj, \"prop\", 456)",
                "Assert2(obj.prop, 456)"
            };

            foreach (var expr in exprs)
            {
                var x = engine.Eval(expr); // Assert failures will throw.
            }
        }

        [Theory]
        [InlineData("Assert2(obj.prop, 123); Set2(obj, \"prop\", 456); Assert2(obj.prop, 456)")]
        [InlineData("Assert2(obj.prop, 123); Set3(obj, \"prop\", \"prop2\"); Assert2(obj.prop, 456)")]
        public void MutabilityTest_Chain(string expr)
        {
            var config = new PowerFxConfig();
            config.AddFunction(new Assert2Function());
            config.AddFunction(new Set2Function());
            config.AddFunction(new Set3Function());
            var engine = new RecalcEngine(config);

            var d = new Dictionary<string, FormulaValue>
            {
                ["prop"] = FormulaValue.New(123),
                ["prop2"] = FormulaValue.New(456)
            };

            var obj = MutableObject.New(d);
            engine.UpdateVariable("obj", obj);

            var x = engine.Eval(expr, options: new ParserOptions() { AllowsSideEffects = true }); // Assert failures will throw.

            if (x is ErrorValue ev)
            {
                throw new XunitException($"FormulaValue is ErrorValue: {string.Join("\r\n", ev.Errors)}");
            }

            Assert.IsType<DecimalValue>(x);
            Assert.Equal(456, ((DecimalValue)x).Value);
            Assert.Equal(456, ((DecimalValue)d["prop"]).Value);
        }

        [Fact]
        public void MutabilityTest_InvalidChain()
        {
            var config = new PowerFxConfig();
            var engine = new RecalcEngine(config);
            var called = false;

            Assert.Throws<InvalidOperationException>(() => engine.SetFormula("A", "1;2", (str, fv) => { called = true; }));
            Assert.False(called);
        }

        private class Assert2Function : ReflectionFunction
        {
            public Assert2Function()
                : base("Assert2", FormulaType.Decimal, new UntypedObjectType(), FormulaType.Decimal)
            {
            }

            public DecimalValue Execute(UntypedObjectValue obj, DecimalValue val)
            {
                var impl = obj.Impl;
                var actual = impl.GetDecimal();
                var expected = val.Value;

                if (actual != expected)
                {
                    throw new InvalidOperationException($"Mismatch");
                }

                return new DecimalValue(IRContext.NotInSource(FormulaType.Decimal), actual);
            }
        }

        private class Set2Function : ReflectionFunction
        {
            public Set2Function()
                : base("Set2", FormulaType.Blank, new UntypedObjectType(), FormulaType.String, FormulaType.Decimal)
            {
            }

            public void Execute(UntypedObjectValue obj, StringValue propName, FormulaValue val)
            {
                var impl = (MutableObject)obj.Impl;
                impl.Set(propName.Value, val);
            }
        }

        private class Set3Function : ReflectionFunction
        {
            public Set3Function()
                : base("Set3", FormulaType.Blank, new UntypedObjectType(), FormulaType.String, FormulaType.String)
            {
            }

            public void Execute(UntypedObjectValue obj, StringValue propName, StringValue propName2)
            {
                var impl = (MutableObject)obj.Impl;
                impl.TryGetProperty(propName2.Value, out var propValue);
                var val = propValue.GetDecimal();
                impl.Set(propName.Value, new DecimalValue(IRContext.NotInSource(FormulaType.Decimal), val));
            }
        }

        private class SimpleObject : IUntypedObject
        {
            private readonly FormulaValue _value;

            public SimpleObject(FormulaValue value)
            {
                _value = value;
            }

            public IUntypedObject this[int index] => throw new NotImplementedException();

            public FormulaType Type => _value.Type;

            public int GetArrayLength()
            {
                throw new NotImplementedException();
            }

            public bool GetBoolean()
            {
                return ((BooleanValue)_value).Value;
            }

            public double GetDouble()
            {
                return ((NumberValue)_value).Value;
            }

            public decimal GetDecimal()
            {
                return ((DecimalValue)_value).Value;
            }

            public string GetUntypedNumber()
            {
                return ((StringValue)_value).Value;
            }

            public string GetString()
            {
                return ((StringValue)_value).Value;
            }

            public bool TryGetProperty(string value, out IUntypedObject result)
            {
                throw new NotImplementedException();
            }

            public bool TryGetPropertyNames(out IEnumerable<string> result)
            {
                result = null;
                return false;
            }
        }

        private class MutableObject : UntypedObjectBase
        {
            private Dictionary<string, FormulaValue> _values = new Dictionary<string, FormulaValue>();

            public void Set(string property, FormulaValue newValue)
            {
                _values[property] = newValue;
            }

            public static UntypedObjectValue New(Dictionary<string, FormulaValue> d)
            {
                var x = new MutableObject()
                {
                    _values = d
                };

                return FormulaValue.New(x);
            }

            public override IUntypedObject this[int index] => throw new NotImplementedException();

            public override FormulaType Type => ExternalType.ObjectType;

            public override int GetArrayLength()
            {
                throw new NotImplementedException();
            }

            public override bool GetBoolean()
            {
                throw new NotImplementedException();
            }

            public override double GetDouble()
            {
                throw new NotImplementedException();
            }

            public override decimal GetDecimal()
            {
                throw new NotImplementedException();
            }

            public override string GetUntypedNumber()
            {
                throw new NotImplementedException();
            }

            public override string GetString()
            {
                throw new NotImplementedException();
            }

            public override bool TryGetProperty(string value, out IUntypedObject result)
            {
                if (_values.TryGetValue(value, out var x))
                {
                    result = new SimpleObject(x);
                    return true;
                }

                result = null;
                return false;
            }

            public override bool TryGetPropertyNames(out IEnumerable<string> result)
            {
                result = null;
                return false;
            }
        }
    }
}
