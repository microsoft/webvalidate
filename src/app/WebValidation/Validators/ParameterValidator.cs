// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using CSE.WebValidate.Model;

namespace CSE.WebValidate.Validators
{
    /// <summary>
    /// Validate parameters
    /// </summary>
    public static class ParameterValidator
    {
        /// <summary>
        /// validate Request
        /// </summary>
        /// <param name="r">Request</param>
        /// <returns>ValidationResult</returns>
        public static ValidationResult Validate(Request r)
        {
            ValidationResult result = new ValidationResult();

            if (r == null)
            {
                result.Failed = true;
                result.ValidationErrors.Add("request is null");
                return result;
            }

            // validate the request path
            result.Add(ValidatePath(r.Path));

            // validate the verb
            result.Add(ValidateVerb(r.Verb));

            // validate the rules
            result.Add(Validate(r.Validation));

            return result;
        }

        /// <summary>
        /// validate Validation
        /// </summary>
        /// <param name="v">Validation</param>
        /// <returns>ValidationResult</returns>
        public static ValidationResult Validate(Validation v)
        {
            ValidationResult res = new ValidationResult();

            // nothing to validate
            if (v == null)
            {
                return res;
            }

            // validate http status code
            if (v.StatusCode < 100 || v.StatusCode > 599)
            {
                res.Failed = true;
                res.ValidationErrors.Add("statusCode: invalid status code: " + v.StatusCode.ToString(CultureInfo.InvariantCulture));
            }

            // validate ContentType
            if (v.ContentType != null && v.ContentType.Length == 0)
            {
                res.Failed = true;
                res.ValidationErrors.Add("contentType: ContentType cannot be empty");
            }

            // validate ExactMatch
            if (v.ExactMatch != null && v.ExactMatch.Length == 0)
            {
                res.Failed = true;
                res.ValidationErrors.Add("exactMatch: exactMatch cannot be empty string");
            }

            // validate lengths
            res.Add(ValidateLength(v));

            // validate MaxMilliSeconds
            if (v.MaxMilliseconds != null && v.MaxMilliseconds <= 0)
            {
                res.Failed = true;
                res.ValidationErrors.Add("maxMilliseconds: maxMilliseconds cannot be less than zero");
            }

            // validate Contains
            res.Add(ValidateContains(v.Contains));

            // validate perfTarget
            res.Add(Validate(v.PerfTarget));

            // validate json object
            res.Add(Validate(v.JsonObject));

            // validate json array parameters
            res.Add(Validate(v.JsonArray));

            return res;
        }

        /// <summary>
        /// validate PerfTarget
        /// </summary>
        /// <param name="target">PerfTarget</param>
        /// <returns>ValidationResult</returns>
        public static ValidationResult Validate(PerfTarget target)
        {
            ValidationResult res = new ValidationResult();

            // null check
            if (target == null)
            {
                return res;
            }

            // validate Category
            if (string.IsNullOrWhiteSpace(target.Category))
            {
                res.Failed = true;
                res.ValidationErrors.Add("category: category cannot be empty");
            }

            // validate Targets
            if (target.Quartiles == null || target.Quartiles.Count != 3)
            {
                res.Failed = true;
                res.ValidationErrors.Add("quartiles: quartiles must have 3 values");
            }

            target.Category = target?.Category?.Trim();

            return res;
        }

        /// <summary>
        /// validate JsonArray
        /// </summary>
        /// <param name="a">JsonArray</param>
        /// <returns>ValidationResult</returns>
        public static ValidationResult Validate(JsonArray a)
        {
            ValidationResult res = new ValidationResult();

            // null check
            if (a == null)
            {
                return res;
            }

            // must be null or >= 0
            if ((a.Count != null && a.Count < 0) ||
                (a.MinCount != null && a.MinCount < 0) ||
                (a.MaxCount != null && a.MaxCount < 0))
            {
                res.Failed = true;
                res.ValidationErrors.Add("jsonArray: count parameters must be >= 0");
            }

            // can't combine Count with MinCount or MaxCount
            if (a.Count != null && (a.MinCount != null || a.MaxCount != null))
            {
                res.Failed = true;
                res.ValidationErrors.Add("jsonArray: cannot combine Count with MinCount or MaxCount");
            }

            // MaxCount must be > MinCount
            if (a.MinCount != null && a.MaxCount != null && a.MinCount >= a.MaxCount)
            {
                res.Failed = true;
                res.ValidationErrors.Add("jsonArray: MaxCount must be > MinCount");
            }

            // validate ForEach
            res.Add(Validate(a.ForEach));

            // validate ByIndex
            res.Add(Validate(a.ByIndex));

            return res;
        }

        /// <summary>
        /// validate JsonObject
        /// </summary>
        /// <param name="jsonobject">list of JsonProperty</param>
        /// <returns>ValidationResult</returns>
        public static ValidationResult Validate(List<JsonItem> jsonobject)
        {
            ValidationResult res = new ValidationResult();

            // null check
            if (jsonobject == null)
            {
                return res;
            }

            // validate field
            foreach (JsonItem f in jsonobject)
            {
                if (string.IsNullOrWhiteSpace(f.Field))
                {
                    res.Failed = true;
                    res.ValidationErrors.Add("field: field cannot be empty");
                }

                res.Add(Validate(f.Validation));
            }

            return res;
        }

        /// <summary>
        /// validate JsonArray.ByIndex
        /// </summary>
        /// <param name="byIndexList">list of JsonPropertyByIndex</param>
        /// <returns>ValidationResult</returns>
        public static ValidationResult Validate(List<JsonPropertyByIndex> byIndexList)
        {
            ValidationResult res = new ValidationResult();

            // null check
            if (byIndexList == null || byIndexList.Count == 0)
            {
                return res;
            }

            // validate parameters
            foreach (JsonPropertyByIndex f in byIndexList)
            {
                // validate index
                if (f.Index < 0)
                {
                    res.Failed = true;
                    res.ValidationErrors.Add("index: index cannot be less than 0");
                }

                // validate field, value, validation
                if (f.Field == null && f.Value == null && f.Validation == null)
                {
                    res.Failed = true;
                    res.ValidationErrors.Add("field: all fields cannot be null");
                }

                // validate recursively
                res.Add(Validate(f.Validation));
            }

            return res;
        }

        /// <summary>
        /// Validate JsonArray.ForEach
        /// </summary>
        /// <param name="list">list of Validation objects</param>
        /// <returns>ValidationResult</returns>
        public static ValidationResult Validate(List<Validation> list)
        {
            ValidationResult res = new ValidationResult();

            if (list == null || list.Count == 0)
            {
                return res;
            }

            // validate recursively
            foreach (Validation v in list)
            {
                res.Add(Validate(v));
            }

            return res;
        }

        /// <summary>
        /// validate Length, MinLength and MaxLength
        /// </summary>
        /// <param name="v">Validation</param>
        /// <returns>ValidationResult</returns>
        public static ValidationResult ValidateLength(Validation v)
        {
            ValidationResult res = new ValidationResult();

            // nothing to validate
            if (v == null)
            {
                return res;
            }

            // validate Length
            if (v.Length != null && v.Length < 0)
            {
                res.ValidationErrors.Add("length: length cannot be empty");
            }

            // validate MinLength
            if (v.MinLength != null && v.MinLength < 0)
            {
                res.ValidationErrors.Add("minlength: minLength cannot be empty");
            }

            // validate MaxLength
            if (v.MaxLength != null)
            {
                if (v.MaxLength < 0)
                {
                    res.ValidationErrors.Add("maxLength: maxLength must be greater than zero");
                }

                if (v.MinLength != null && v.MaxLength <= v.MinLength)
                {
                    res.ValidationErrors.Add("maxLength: maxLength must be greater than minLength");
                }
            }

            return res;
        }

        /// <summary>
        /// validate Verb
        /// </summary>
        /// <param name="verb">string</param>
        /// <returns>ValidationResult</returns>
        public static ValidationResult ValidateVerb(string verb)
        {
            ValidationResult res = new ValidationResult();

            if (!string.IsNullOrEmpty(verb))
            {
                verb = verb.Trim().ToUpperInvariant();
            }

            // verb must be in this list
            if (!new List<string> { "GET", "HEAD", "POST", "PUT", "DELETE", "TRACE", "OPTIONS", "CONNECT", "PATCH" }.Contains(verb))
            {
                res.ValidationErrors.Add("verb: invalid verb: " + verb);
            }

            return res;
        }

        /// <summary>
        /// validate Contains
        /// </summary>
        /// <param name="contains">list of string</param>
        /// <returns>ValidationResult</returns>
        public static ValidationResult ValidateContains(List<string> contains)
        {
            ValidationResult res = new ValidationResult();

            // null check
            if (contains == null || contains.Count == 0)
            {
                return res;
            }

            // validate each value
            foreach (string c in contains)
            {
                if (string.IsNullOrEmpty(c))
                {
                    res.ValidationErrors.Add("contains: values cannot be empty");
                }
            }

            return res;
        }

        /// <summary>
        /// validate Path
        /// </summary>
        /// <param name="path">string</param>
        /// <returns>ValidationResult</returns>
        public static ValidationResult ValidatePath(string path)
        {
            ValidationResult res = new ValidationResult();

            // path is required
            if (string.IsNullOrWhiteSpace(path))
            {
                res.Failed = true;
                res.ValidationErrors.Add("path: path is required");
            }

            // path must begin with /
            else if (!path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                res.Failed = true;
                res.ValidationErrors.Add("path: path must begin with /");
            }

            return res;
        }

        /// <summary>
        /// validate NotContains
        /// </summary>
        /// <param name="notcontains">list of string</param>
        /// <returns>ValidationResult</returns>
        public static ValidationResult ValidateNotContains(List<string> notcontains)
        {
            ValidationResult res = new ValidationResult();

            // null check
            if (notcontains == null || notcontains.Count == 0)
            {
                return res;
            }

            // validate each value
            foreach (string c in notcontains)
            {
                if (string.IsNullOrEmpty(c))
                {
                    res.Failed = true;
                    res.ValidationErrors.Add("notContains: values cannot be empty");
                }
            }

            return res;
        }
    }
}
