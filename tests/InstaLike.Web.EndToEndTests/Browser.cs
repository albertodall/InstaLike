using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Dom;
using Xunit;

namespace InstaLike.Web.EndToEndTests
{
    /// <summary>
    /// Navigates the application like a browser without JavaScript: it keeps the cookies and submits the forms of the pages,
    /// antiforgery token included, and posts the requests of its scripts with the token they read from the page.
    /// Redirects are not followed automatically, so that tests can check where they lead.
    /// </summary>
    public sealed class Browser : IDisposable
    {
        private readonly HttpClient _client;

        public Browser(HttpClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Opens a page, failing unless the response is 200.
        /// </summary>
        public async Task<IHtmlDocument> OpenAsync(string url)
        {
            var response = await GetAsync(url);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException($"Opening '{url}' returned {(int)response.StatusCode} instead of a page.");
            }

            return await ReadPageAsync(response);
        }

        /// <summary>
        /// Sends a GET and returns the response as it is, redirects included.
        /// </summary>
        public Task<HttpResponseMessage> GetAsync(string url)
        {
            return _client.GetAsync(url, TestContext.Current.CancellationToken);
        }

        /// <summary>
        /// Opens the page a redirect points to.
        /// </summary>
        public Task<IHtmlDocument> FollowRedirectAsync(HttpResponseMessage response)
        {
            var location = response.Headers.Location;
            if (location is null)
            {
                throw new InvalidOperationException($"The response is {(int)response.StatusCode}, not a redirect.");
            }

            return OpenAsync(location.OriginalString);
        }

        /// <summary>
        /// Submits a form with the values it already has.
        /// </summary>
        public Task<HttpResponseMessage> SubmitAsync(IHtmlFormElement form)
        {
            return SubmitAsync(form, new Dictionary<string, string>());
        }

        /// <summary>
        /// Submits a form, replacing the values of the given fields.
        /// </summary>
        public Task<HttpResponseMessage> SubmitAsync(IHtmlFormElement form, IReadOnlyDictionary<string, string> fields)
        {
            var values = SubmittedValues(form, fields);
            if (form.Enctype != "multipart/form-data")
            {
                return _client.PostAsync(
                    form.Action,
                    new FormUrlEncodedContent(values),
                    TestContext.Current.CancellationToken);
            }

            var content = MultipartContent(values);
            AddEmptyFileInputs(content, form);

            return _client.PostAsync(
                form.Action,
                content,
                TestContext.Current.CancellationToken);
        }

        /// <summary>
        /// Submits a form, replacing the values of the given fields and choosing the test picture in its picture field.
        /// </summary>
        public Task<HttpResponseMessage> SubmitWithPictureAsync(IHtmlFormElement form, IReadOnlyDictionary<string, string> fields, string pictureField)
        {
            var content = MultipartContent(SubmittedValues(form, fields));
            AddTestPicture(content, pictureField);

            return _client.PostAsync(
                form.Action,
                content,
                TestContext.Current.CancellationToken);
        }

        /// <summary>
        /// Posts fields as a script of the page does, with the antiforgery token in the request header.
        /// </summary>
        public Task<HttpResponseMessage> PostFromScriptAsync(IHtmlDocument page, string url, IReadOnlyDictionary<string, string> fields)
        {
            return PostFromScriptAsync(page, url, new FormUrlEncodedContent(fields));
        }

        /// <summary>
        /// Uploads the test picture in the given field as a script of the page does,
        /// with the antiforgery token in the request header.
        /// </summary>
        public Task<HttpResponseMessage> UploadPictureFromScriptAsync(IHtmlDocument page, string url, string pictureField)
        {
            var content = new MultipartFormDataContent();
            AddTestPicture(content, pictureField);

            return PostFromScriptAsync(page, url, content);
        }

        /// <summary>
        /// Parses the HTML of a response, so that relative links and form actions resolve against its URL.
        /// </summary>
        public static async Task<IHtmlDocument> ReadPageAsync(HttpResponseMessage response)
        {
            var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var address = response.RequestMessage?.RequestUri?.ToString() ?? "https://localhost/";
            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(
                request => request.Content(html).Address(address),
                TestContext.Current.CancellationToken);

            return (IHtmlDocument)document;
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        /// <summary>
        /// Reads the token the layout renders for scripts and sends it in the header they use.
        /// </summary>
        private Task<HttpResponseMessage> PostFromScriptAsync(IHtmlDocument page, string url, HttpContent content)
        {
            var token = page.QuerySelector("meta[name='request-verification-token']")?.GetAttribute("content");
            if (token is null)
            {
                throw new InvalidOperationException("The page has no antiforgery token for its scripts.");
            }

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            request.Headers.Add("RequestVerificationToken", token);

            return _client.SendAsync(request, TestContext.Current.CancellationToken);
        }

        /// <summary>
        /// The values a browser would send for the form fields, with the given fields replaced.
        /// </summary>
        private static List<KeyValuePair<string, string>> SubmittedValues(IHtmlFormElement form, IReadOnlyDictionary<string, string> fields)
        {
            return FormValues(form)
                .Where(field => !fields.ContainsKey(field.Key))
                .Concat(fields)
                .ToList();
        }

        /// <summary>
        /// The values a browser would send for the form fields.
        /// </summary>
        private static IEnumerable<KeyValuePair<string, string>> FormValues(IHtmlFormElement form)
        {
            foreach (var element in form.Elements)
            {
                switch (element)
                {
                    case IHtmlInputElement input when !string.IsNullOrEmpty(input.Name) && IsSubmitted(input):
                        yield return new(input.Name, input.Value);
                        break;
                    case IHtmlTextAreaElement textArea when !string.IsNullOrEmpty(textArea.Name):
                        yield return new(textArea.Name, textArea.Value);
                        break;
                    case IHtmlSelectElement select when !string.IsNullOrEmpty(select.Name):
                        yield return new(select.Name, select.Value ?? string.Empty);
                        break;
                }
            }
        }

        /// <summary>
        /// Whether a browser sends the input: files are attached separately, buttons are not sent, unchecked boxes are left out.
        /// </summary>
        private static bool IsSubmitted(IHtmlInputElement input)
        {
            return input.Type switch
            {
                "file" or "submit" or "button" or "image" or "reset" => false,
                "checkbox" or "radio" => input.IsChecked,
                _ => true
            };
        }

        /// <summary>
        /// Builds a multipart body with the field values.
        /// </summary>
        private static MultipartFormDataContent MultipartContent(IEnumerable<KeyValuePair<string, string>> values)
        {
            var content = new MultipartFormDataContent();
            foreach (var value in values)
            {
                content.Add(new StringContent(value.Value), value.Key);
            }

            return content;
        }

        /// <summary>
        /// Adds what a browser sends for file inputs left without a file:
        /// an empty part with an empty file name.
        /// </summary>
        private static void AddEmptyFileInputs(MultipartFormDataContent content, IHtmlFormElement form)
        {
            var fileInputs = form.Elements.OfType<IHtmlInputElement>()
                .Where(input => input.Type == "file" && !string.IsNullOrEmpty(input.Name));
            foreach (var input in fileInputs)
            {
                var noFile = new ByteArrayContent([]);
                noFile.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                noFile.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = $"\"{input.Name}\"",
                    FileName = "\"\""
                };
                content.Add(noFile);
            }
        }

        private static void AddTestPicture(MultipartFormDataContent content, string fieldName)
        {
            var picture = new ByteArrayContent(TestPicture.Content);
            picture.Headers.ContentType = new MediaTypeHeaderValue(TestPicture.MediaType);
            content.Add(picture, fieldName, TestPicture.FileName);
        }
    }
}
