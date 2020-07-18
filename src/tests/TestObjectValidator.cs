using CSE.WebValidate.Model;
using CSE.WebValidate.Parameters;
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
            Assert.False(Validator.Validate(properties).Failed);

            // Field can't be empty
            properties = new List<JsonProperty> { new JsonProperty { Field = string.Empty } };
            Assert.True(Validator.Validate(properties).Failed);

            // valid
            properties.Clear();
            properties.Add(new JsonProperty { Field = "type" });
            Assert.False(Validator.Validate(properties).Failed);

            // validate empty list
            properties = new List<JsonProperty>();
            Assert.Empty(properties);
            Assert.False(Validator.Validate(properties).Failed);

            // validate adding a property
            properties.Add(new JsonProperty());
            Assert.True(Validator.Validate(properties).Failed);
        }
    }
}
