using System.Collections.Generic;
using CSE.WebValidate.Model;
using CSE.WebValidate.Validators;
using Xunit;

namespace CSE.WebValidate.Tests.Unit
{
    public class TestJsonObjectValidator
    {
        [Fact]
        public void JsonObjectTest()
        {
            List<JsonItem> properties = null;

            // validate json object is null
            Assert.False(ParameterValidator.Validate(properties).Failed);

            // Field can't be empty
            properties = new List<JsonItem> { new JsonItem { Field = string.Empty } };
            Assert.True(ParameterValidator.Validate(properties).Failed);

            // valid
            properties.Clear();
            properties.Add(new JsonItem { Field = "type" });
            Assert.False(ParameterValidator.Validate(properties).Failed);

            // validate empty list
            properties = new List<JsonItem>();
            Assert.Empty(properties);
            Assert.False(ParameterValidator.Validate(properties).Failed);

            // validate adding a property
            properties.Add(new JsonItem());
            Assert.True(ParameterValidator.Validate(properties).Failed);
        }
    }
}
