using CSE.WebValidate.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Runtime.Serialization;

namespace CSE.WebValidate.Response
{
    public static class Validator
    {
        // validate Request
        public static ValidationResult Validate(Request r, HttpResponseMessage response, string body)
        {
            ValidationResult result = new ValidationResult();

            if (r == null || r.Validation == null)
            {
                return result;
            }

            if (response == null)
            {
                result.ValidationErrors.Add("validate: null http response message");
                result.Failed = true;
                return result;
            }

            // validate status code - fail on error
            result.Add(ValidateStatusCode((int)response.StatusCode, r.Validation.StatusCode));

            // don't validate further if the status code is wrong
            if (result.Failed)
            {
                return result;
            }

            // redirects don't have body or headers
            if ((int)response.StatusCode >= 300 && (int)response.StatusCode <= 399)
            {
                return result;
            }

            // handle framework 4xx status codes
            if (response.Content?.Headers?.ContentType == null)
            {
                return result;
            }

            // validate ContentType - fail on error
            result.Add(ValidateContentType(response.Content.Headers.ContentType.ToString(), r.Validation.ContentType));

            // don't validate further if the content type is wrong
            if (result.Failed)
            {
                return result;
            }

            // make sure the validators don't throw an exception but still fail
            if (body == null)
            {
                body = string.Empty;
            }

            // run validation rules
            result.Add(ValidateLength((long)response.Content.Headers.ContentLength, r.Validation));
            result.Add(Validate(r.Validation, body));

            // check FailOnValidationError
            if (r.FailOnValidationError)
            {
                if (result.ValidationErrors.Count > 0)
                {
                    result.Failed = true;
                }
            }

            return result;
        }

        // validate Validation
        public static ValidationResult Validate(Validation v, string body)
        {
            ValidationResult result = new ValidationResult();

            if (v != null)
            {
                // make sure the validators don't throw an exception but still fail
                if (body == null)
                {
                    body = string.Empty;
                }

                result.Add(ValidateContains(v.Contains, body));
                result.Add(ValidateNotContains(v.NotContains, body));
                result.Add(ValidateExactMatch(v.ExactMatch, body));
                result.Add(Validate(v.JsonObject, body));

                result.Add(Validate(v.JsonArray, body));
            }

            return result;
        }

        // validate JsonObject
        public static ValidationResult Validate(List<JsonProperty> properties, string body)
        {
            ValidationResult result = new ValidationResult();

            // nothing to check
            if (properties == null || properties.Count == 0)
            {
                return result;
            }

            // make sure the validators don't throw an exception but still fail
            if (body == null)
            {
                body = string.Empty;
            }

            try
            {
                // deserialize the json into an IDictionary
                IDictionary<string, object> dict = JsonConvert.DeserializeObject<ExpandoObject>(body);

                // set to new so validation fails
                if (dict == null)
                {
                    dict = new Dictionary<string, object>();
                }

                foreach (JsonProperty property in properties)
                {
                    if (!string.IsNullOrEmpty(property.Field) && dict.ContainsKey(property.Field))
                    {
                        if (property.Validation != null)
                        {
                            if (dict[property.Field] == null)
                            {
                                result.ValidationErrors.Add($"json: Field is null: {property.Field}");
                            }
                            else
                            {
                                result.Add(Validate(property.Validation, JsonConvert.SerializeObject(dict[property.Field])));
                            }
                        }

                        // null values check for the existance of the field in the payload
                        // used when values are not known
                        if (property.Value != null && !dict[property.Field].Equals(property.Value))
                        {
                            // whole numbers map to int
                            if (!((property.Value.GetType() == typeof(double) ||
                                property.Value.GetType() == typeof(float) ||
                                property.Value.GetType() == typeof(decimal)) &&
                                double.TryParse(dict[property.Field].ToString(), out double d) &&
                                (double)property.Value == d))
                            {
                                result.ValidationErrors.Add($"json: {property.Field}: {dict[property.Field]} : Expected: {property.Value}");
                            }
                        }
                    }
                    else
                    {
                        result.ValidationErrors.Add($"json: Field Not Found: {property.Field}");
                    }
                }
            }

            catch (SerializationException se)
            {
                result.ValidationErrors.Add($"Exception: {se.Message}");
            }

            catch (Exception ex)
            {
                result.ValidationErrors.Add($"Exception: {ex.Message}");
            }

            return result;
        }

        // validate JsonArray
        public static ValidationResult Validate(JsonArray jArray, string body)
        {
            ValidationResult result = new ValidationResult();

            if (jArray == null)
            {
                return result;
            }

            // make sure the validators don't throw an exception but still fail
            if (body == null)
            {
                body = string.Empty;
            }

            try
            {
                // deserialize the json
                List<dynamic> resList = JsonConvert.DeserializeObject<List<dynamic>>(body);

                result.Add(ValidateJsonArrayLength(jArray, resList));
                result.Add(ValidateForEach(jArray.ForEach, resList));

                result.Add(ValidateByIndex(jArray.ByIndex, resList));
            }
            catch (SerializationException se)
            {
                result.ValidationErrors.Add($"Exception: {se.Message}");
            }

            catch (Exception ex)
            {
                result.ValidationErrors.Add($"Exception: {ex.Message}");
            }

            return result;
        }

        // validate ForEach
        private static ValidationResult ValidateForEach(List<Validation> validationList, List<dynamic> documentList)
        {
            ValidationResult result = new ValidationResult();

            // validate foreach items recursively
            if (validationList != null && validationList.Count > 0)
            {
                foreach (dynamic doc in documentList)
                {
                    // run each validation on each doc
                    foreach (Validation fe in validationList)
                    {
                        // call validate recursively
                        result.Add(Validate(fe, JsonConvert.SerializeObject(doc)));
                    }
                }
            }

            return result;
        }

        // validate ByIndex
        private static ValidationResult ValidateByIndex(List<JsonPropertyByIndex> byIndexList, List<dynamic> documentList)
        {
            ValidationResult result = new ValidationResult();

            // validate array items by index
            if (byIndexList != null && byIndexList.Count > 0)
            {
                string fieldBody;
                double d = 0;
                int ndx = -1;

                foreach (JsonPropertyByIndex property in byIndexList)
                {
                    ndx++;

                    // check index in bounds
                    if (property.Index < 0 || property.Index >= documentList.Count)
                    {
                        result.ValidationErrors.Add($"byIndex: Index out of bounds: {property.Index}");
                        break;
                    }

                    // validate recursively
                    if (property.Validation != null)
                    {
                        // set the body to entire doc or field
                        if (property.Field == null)
                        {
                            fieldBody = JsonConvert.SerializeObject(documentList[(int)property.Index]);
                        }
                        else
                        {
                            fieldBody = JsonConvert.SerializeObject(documentList[(int)property.Index][property.Field]);
                        }

                        // validate recursively
                        result.Add(Validate(property.Validation, fieldBody));
                    }
                    else if (!string.IsNullOrEmpty(property.Field) && property.Value != null)
                    {
                        // null values check for the existance of the field in the payload
                        // used when values are not known
                        if (documentList[(int)property.Index][property.Field] != property.Value)
                        {
                            // whole numbers map to int
                            if (!((property.Value.GetType() == typeof(double) ||
                                property.Value.GetType() == typeof(float) ||
                                property.Value.GetType() == typeof(decimal)) &&
                                double.TryParse(documentList[(int)property.Index][property.Field].ToString(), out d) &&
                                (double)property.Value == d))
                            {
                                result.ValidationErrors.Add($"json: {property.Field}: {documentList[(int)property.Index][property.Field]} : Expected: {property.Value}");
                            }
                        }
                    }
                    else if (property.Value != null)
                    {
                        // used for checking array of simple type
                        if (!property.Value.Equals(documentList[(int)property.Index]))
                        {
                            result.ValidationErrors.Add($"json: {property.Field}: {documentList[(int)property.Index]} : Expected: {property.Value}");
                        }
                    }
                }
            }

            return result;
        }

        // validate JsonArray Length, MinLength and MaxLength
        private static ValidationResult ValidateJsonArrayLength(JsonArray jArray, List<dynamic> documentList)
        {
            ValidationResult result = new ValidationResult();

            // validate count
            if (jArray.Count != null && jArray.Count != documentList.Count)
            {
                result.ValidationErrors.Add($"JsonArrayCount: {documentList.Count} Expected: {jArray.Count}");
            }

            // validate min count
            if (jArray.MinCount != null && jArray.MinCount > documentList.Count)
            {
                result.ValidationErrors.Add($"MinJsonCount: {documentList.Count} Expected: {jArray.MinCount}");
            }

            // validate max count
            if (jArray.MaxCount != null && jArray.MaxCount < documentList.Count)
            {
                result.ValidationErrors.Add($"MaxJsonCount: {documentList.Count} Expected: {jArray.MaxCount}");
            }

            return result;
        }

        // validate StatusCode
        public static ValidationResult ValidateStatusCode(int actual, int expected)
        {
            ValidationResult result = new ValidationResult();

            if (actual != expected)
            {
                result.Failed = true;
                result.ValidationErrors.Add($"StatusCode: {actual} Expected: {expected}");
            }

            return result;
        }

        // validate ContentType
        public static ValidationResult ValidateContentType(string actual, string expected)
        {
            ValidationResult result = new ValidationResult();

            if (!string.IsNullOrEmpty(expected))
            {
                if (actual != null && !actual.StartsWith(expected, StringComparison.OrdinalIgnoreCase))
                {
                    result.Failed = true;
                    result.ValidationErrors.Add($"ContentType: {actual} Expected: {expected}");
                }
            }

            return result;
        }

        // validate Length, MinLength and MaxLength
        public static ValidationResult ValidateLength(long actual, Validation v)
        {
            ValidationResult result = new ValidationResult();

            // nothing to validate
            if (v == null || (v.Length == null && v.MinLength == null && v.MaxLength == null))
            {
                return result;
            }

            // validate length
            if (v.Length != null)
            {
                if (actual != v.Length)
                {
                    result.ValidationErrors.Add($"Length: {actual} Expected: {v.Length}");
                }
            }

            // validate minLength
            if (v.MinLength != null)
            {
                if (actual < v.MinLength)
                {
                    result.ValidationErrors.Add($"MinContentLength: {actual} Expected: {v.MinLength}");
                }
            }

            // validate maxLength
            if (v.MaxLength != null)
            {
                if (actual > v.MaxLength)
                {
                    result.ValidationErrors.Add($"MaxContentLength: {actual} Expected: {v.MaxLength}");
                }
            }

            return result;
        }

        // validate ExactMatch
        public static ValidationResult ValidateExactMatch(string exactMatch, string body)
        {
            ValidationResult result = new ValidationResult();

            // nothing to validate
            if (exactMatch == null)
            {
                return result;
            }

            // make sure the validators don't throw an exception but still fail
            if (body == null)
            {
                body = string.Empty;
            }

            // compare values
            if (body != exactMatch)
            {
                result.ValidationErrors.Add($"ExactMatch: {body} : Expected: {exactMatch}");
            }

            return result;
        }

        // validate Contains
        public static ValidationResult ValidateContains(List<string> containsList, string body)
        {
            ValidationResult result = new ValidationResult();

            if (containsList == null || containsList.Count == 0)
            {
                return result;
            }

            // make sure the validators don't throw an exception but still fail
            if (body == null)
            {
                body = string.Empty;
            }

            // validate each rule
            foreach (string c in containsList)
            {
                // compare values
                if (!body.Contains(c, StringComparison.InvariantCulture))
                {
                    result.ValidationErrors.Add($"Contains: {c}");
                }
            }

            return result;
        }

        //validate NotContains
        public static ValidationResult ValidateNotContains(List<string> notContainsList, string body)
        {
            ValidationResult result = new ValidationResult();

            // nothing to validate
            if (notContainsList == null || notContainsList.Count == 0 || string.IsNullOrEmpty(body))
            {
                return result;
            }

            // validate each rule
            foreach (string c in notContainsList)
            {
                // compare values
                if (body.Contains(c, StringComparison.InvariantCulture))
                {
                    result.ValidationErrors.Add($"NotContains: {c}");
                }
            }

            return result;
        }
    }
}
