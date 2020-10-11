using System.Collections.Generic;
using CSE.WebValidate.Model;
using CSE.WebValidate.Validators;
using Xunit;

namespace CSE.WebValidate.Tests.Unit
{
    public class TestArrayValidator
    {
        [Fact]
        public void JsonArrayTest()
        {
            ValidationResult res;
            JsonArray a;

            // validate empty array
            a = new JsonArray();
            res = ParameterValidator.Validate(a);
            Assert.False(res.Failed);

            // validate bad count
            a = new JsonArray
            {
                Count = -1
            };
            res = ParameterValidator.Validate(a);
            Assert.True(res.Failed);

            // validate bad count
            a = new JsonArray
            {
                Count = 1,
                MinCount = 1
            };
            res = ParameterValidator.Validate(a);
            Assert.True(res.Failed);

            // validate bad count
            a = new JsonArray
            {
                MaxCount = 1,
                MinCount = 1
            };
            res = ParameterValidator.Validate(a);
            Assert.True(res.Failed);
        }

        [Fact]
        public void ByIndexTest()
        {
            List<JsonPropertyByIndex> list = new List<JsonPropertyByIndex>();
            JsonPropertyByIndex f;

            // empty list is valid
            Assert.False(ParameterValidator.Validate(list).Failed);

            // validate index < 0 fails
            f = new JsonPropertyByIndex
            {
                Index = -1,
                Value = null,
                Validation = null
            };
            list.Add(f);
            Assert.True(ParameterValidator.Validate(list).Failed);

            // validate field, value, validation
            f = new JsonPropertyByIndex
            {
                Index = 0,
                Field = null,
                Value = null,
                Validation = null
            };
            list.Clear();
            list.Add(f);
            Assert.True(ParameterValidator.Validate(list).Failed);
        }
    }
}
