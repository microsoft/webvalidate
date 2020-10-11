using System.Collections.Generic;
using CSE.WebValidate.Model;
using CSE.WebValidate.Validators;
using Xunit;

namespace CSE.WebValidate.Tests.Unit
{
    public class TestCommonTarget
    {
        [Fact]
        public void PathTest()
        {
            ValidationResult res;

            // empty path
            res = ParameterValidator.ValidatePath(string.Empty);
            Assert.True(res.Failed);

            // path must start with /
            res = ParameterValidator.ValidatePath("testpath");
            Assert.True(res.Failed);
        }

        [Fact]
        public void CommonBoundariesTest()
        {
            ValidationResult res;

            // verb must be GET POST PUT DELETE ...
            // path must start with /
            Request r = new Request
            {
                Verb = "badverb",
                Path = "badpath",
                Validation = null
            };
            res = ParameterValidator.Validate(r);
            Assert.True(res.Failed);

            Validation v = new Validation();

            // null is valid
            res = ParameterValidator.ValidateLength(null);
            Assert.False(res.Failed);

            // edge values
            // >= 0
            v.Length = -1;
            v.MinLength = -1;
            v.MaxLength = -1;

            // 200 - 599
            v.StatusCode = 10;

            // > 0
            v.MaxMilliseconds = 0;

            // ! isnullorempty
            v.ExactMatch = string.Empty;
            v.ContentType = string.Empty;

            // each element ! isnullempty
            v.Contains = new List<string> { string.Empty };
            v.NotContains = new List<string> { string.Empty };

            res = ParameterValidator.Validate(v);
            Assert.True(res.Failed);
        }

        [Fact]
        public void PerfLogTest()
        {
            PerfLog p = new PerfLog(new List<string> { "test" })
            {
                Date = new System.DateTime(2020, 1, 1)
            };

            // validate getters and setters
            Assert.Equal(new System.DateTime(2020, 1, 1), p.Date);
            Assert.Single(p.Errors);
            Assert.Equal("test", p.Errors[0]);
        }

        [Fact]
        public void PerfTargetTest()
        {
            ValidationResult res;

            // category can't be blank
            PerfTarget t = new PerfTarget();
            res = ParameterValidator.Validate(t);
            Assert.True(res.Failed);

            // quartiles can't be null
            t.Category = "Tests";
            res = ParameterValidator.Validate(t);
            Assert.True(res.Failed);

            // valid
            t.Quartiles = new List<double> { 100, 200, 400 };
            res = ParameterValidator.Validate(t);
            Assert.False(res.Failed);

        }

        [Fact]
        public void ResponseNullTest()
        {
            Request r = new Request();

            Assert.False(ResponseValidator.Validate(r, null, string.Empty).Failed);

            r.Validation = new Validation();

            Assert.True(ResponseValidator.Validate(r, null, "this is a test").Failed);

            using System.Net.Http.HttpResponseMessage resp = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            Assert.True(ResponseValidator.Validate(r, resp, "this is a test").Failed);

            Assert.True(ResponseValidator.ValidateStatusCode(400, 200).Failed);
        }
    }
}
