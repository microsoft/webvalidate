using CSE.WebValidate.Model;
using CSE.WebValidate.Validators;
using System.Collections.Generic;
using Xunit;

namespace CSE.WebValidate.Tests.Unit
{
    public class TestJsonObjectValidator
    {
        [Fact]
        public void JsonObjectTest()
        {
            List<JsonProperty> properties = null;

            // validate json object is null
            Assert.False(ParameterValidator.Validate(properties).Failed);

            // Field can't be empty
            properties = new List<JsonProperty> { new JsonProperty { Field = string.Empty } };
            Assert.True(ParameterValidator.Validate(properties).Failed);

            // valid
            properties.Clear();
            properties.Add(new JsonProperty { Field = "type" });
            Assert.False(ParameterValidator.Validate(properties).Failed);

            // validate empty list
            properties = new List<JsonProperty>();
            Assert.Empty(properties);
            Assert.False(ParameterValidator.Validate(properties).Failed);

            // validate adding a property
            properties.Add(new JsonProperty());
            Assert.True(ParameterValidator.Validate(properties).Failed);
        }
    }
}
