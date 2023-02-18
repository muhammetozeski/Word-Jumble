// This code is from: https://gist.github.com/ZodmanPerth/be313b8989c7ef191e66b1b0292c2e71?ts=4#file-googleformssubmissionservice-cs-L1
// Thank you ZodmanPerth

/* Usage:
    var fields = new Dictionary<string, string>
    {
        { "entry.11111", "My text" },
        { "entry.22222", "Other text"},
        { "entry.33333", "LINQPad" },
    };

    var formUrl = "https://docs.google.com/forms/d/e/{YourFormCodeHere}/formResponse";
    var submissionService = new GoogleFormsSubmissionService(formUrl);
    submissionService.SetFieldValues(fields);
    submissionService.SetCheckboxValues("entry.44444", "One fish", "Two fish");		//Multiple checkbox values
    submissionService.SetCheckboxValues("entry.55555", "There can be only one");	//Single checkbox value

    var response = await submissionService.SubmitAsync();
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>Submit form data to a Google form</summary>
/// <remarks>Kudos: https://stackoverflow.com/questions/52398172/how-to-fill-google-form-with-c-sharp </remarks>
public class GoogleFormsSubmissionService
{
    private string _baseUrl;
    private Dictionary<string, string> _field;
    private Dictionary<string, string[]> _checkbox;
    private HttpClient _client;

    public GoogleFormsSubmissionService(string formUrl)
    {
        if (string.IsNullOrEmpty(formUrl)) throw new ArgumentNullException(nameof(formUrl));
        _baseUrl = formUrl;

        _field = new Dictionary<string, string>();
        _checkbox = new Dictionary<string, string[]>();
        _client = new HttpClient();
    }

    /// <summary>Set multiple fields with individual values. Fields with empty values will be ignored.</summary>
    /// <remarks>All keys must be valid or exceptions will be thrown.</remarks>
    public void SetFieldValues(Dictionary<string, string> data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (data.Keys.Any(value => string.IsNullOrWhiteSpace(value))) MonoBehaviour.print("" + nameof(data) + "Empty keys are invalid"); //throw new ArgumentNullException(nameof(data), "Empty keys are invalid");

        var fieldsWithData = data.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value));
        foreach (var kvp in fieldsWithData)
            _field[kvp.Key] = kvp.Value;
    }

    /// <summary>Set one or more values for a single checkbox field.  Values must match the text on the form Checkboxes.  Empty values will be ignored.</summary>
    public void SetCheckboxValues(string key, params string[] values)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

        var valuesWithData = values.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();
        _checkbox[key] = valuesWithData;
    }

    /// <summary>Submits the previously set data asynchronously and returns the response.</summary>
    /// <remarks>See https://stackoverflow.com/a/52404185/117797 for queryParams formatting details</remarks>
    public async Task<bool> SubmitAsync()
    {
        try
        {
            if (!_field.Any() && !_checkbox.Any())
            {
                //throw new InvalidOperationException("No data has been set to submit");
                MonoBehaviour.print("No data has been set to submit");
            }

            var content = new FormUrlEncodedContent(_field);
            var url = _baseUrl;
            if (_checkbox.Any())
            {
                var queryParams = string.Join("&", _checkbox.Keys.SelectMany(key => _checkbox[key].Select(value => $"{key}={value.Replace(' ', '+')}")));
                url += $"?{queryParams}";
            }

            var response = await _client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                MonoBehaviour.print("Google form data was submitted sucessfully");
            }
            else
            {
                var fieldText = string.Join(Environment.NewLine, _field.Keys.Select(key => $"{key}={string.Join(",", _field[key])}"));
                MonoBehaviour.print($"Failed to submit Google form\n{response.StatusCode}: {response.ReasonPhrase}\n{url}\n{fieldText}");
            }

            return response.IsSuccessStatusCode;

        }
        catch
        {
            return false;
        }
    }
}