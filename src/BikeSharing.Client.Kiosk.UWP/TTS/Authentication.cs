//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
//
// Microsoft Cognitive Services (formerly Project Oxford): https://www.microsoft.com/cognitive-services
//
// Microsoft Cognitive Services (formerly Project Oxford) GitHub:
// https://github.com/Microsoft/Cognitive-Speech-TTS
//
// Copyright (c) Microsoft Corporation
// All rights reserved.
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TTSSample
{
    /// <summary>
    /// This class demonstrates how to get a valid O-auth token
    /// </summary>
    public class Authentication
    {
        public static readonly string AccessUri = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
        private string apiKey;
        private string accessToken;
        private Timer accessTokenRenewer;

        //Access token expires every 10 minutes. Renew it every 9 minutes only.
        private const int RefreshTokenDuration = 9;

        public Authentication(string apiKey)
        {
            this.apiKey = apiKey;

            // renew the token every specfied minutes
            accessTokenRenewer = new Timer(new TimerCallback(OnTokenExpiredCallback),
                                           this,
                                           TimeSpan.FromMinutes(RefreshTokenDuration),
                                           TimeSpan.FromMilliseconds(-1));
        }

        public async Task<string> GetAccessTokenAsync()
        {
            this.accessToken = await HttpPostAsync(AccessUri, this.apiKey).ConfigureAwait(false);

            return this.accessToken;
        }

        private async Task RenewAccessToken()
        {
            string newAccessToken = await HttpPostAsync(AccessUri, this.apiKey);
            //swap the new token with old one
            //Note: the swap is thread unsafe
            this.accessToken = newAccessToken;
            Debug.WriteLine(string.Format("[Text to Speech]: Renewed token for user: {0} is: {1}",
                              this.apiKey,
                              this.accessToken));
        }

        private async void OnTokenExpiredCallback(object stateInfo)
        {
            try
            {
                await RenewAccessToken();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("[Text to Speech]: Failed renewing access token. Details: {0}", ex.Message));
            }
            finally
            {
                try
                {
                    accessTokenRenewer.Change(TimeSpan.FromMinutes(RefreshTokenDuration), TimeSpan.FromMilliseconds(-1));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("[Text to Speech]: Failed to reschedule the timer to renew access token. Details: {0}", ex.Message));
                }
            }
        }

        private async Task<string> HttpPostAsync(string accessUri, string apiKey)
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(accessUri));
            request.Headers.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", apiKey);
            request.Content = new StringContent(string.Empty);
            HttpResponseMessage response = null;
            await client.SendAsync(request).ContinueWith(responseTask =>
            {
                response = responseTask.Result;
            });

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                Debug.WriteLine("[Text to Speech]: Error retrieving access token.");
                return string.Empty;
            }
        }
    }
}
