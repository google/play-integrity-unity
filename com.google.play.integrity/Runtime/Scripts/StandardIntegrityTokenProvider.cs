// Copyright 2023 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Google.Play.Common;
using Google.Play.Core.Internal;
using Google.Play.Integrity.Internal;
using UnityEngine;

namespace Google.Play.Integrity
{
    /// <summary>
    /// Standard integrity token provider.
    ///
    /// <para>Response of <see cref="StandardIntegrityManager.PrepareIntegrityToken"/> call.</para>
    /// </summary>
    /// <remarks>
    /// DEPRECATED: Please use <see cref="StandardIntegrityManagerV2.StandardIntegrityTokenProvider"/> instead.
    /// </remarks>
    [Obsolete("StandardIntegrityTokenProvider is deprecated. Please use StandardIntegrityManagerV2.StandardIntegrityTokenProvider instead, which provides enhanced error handling and native remediation dialog support.")]
    public class StandardIntegrityTokenProvider
    {
        private readonly PlayCoreStandardIntegrityTokenProvider _playCoreStandardIntegrityTokenProvider;

        private const string StandardIntegrityTokenRequestClassName =
            PlayCoreConstants.IntegrityPackagePrefix + "StandardIntegrityManager$StandardIntegrityTokenRequest";

        internal StandardIntegrityTokenProvider(PlayCoreStandardIntegrityTokenProvider tokenProvider)
        {
            _playCoreStandardIntegrityTokenProvider = tokenProvider;
        }

        /// <summary>
        /// Returns a token for integrity-related enquiries.
        /// </summary>
        /// <param name="request">The object to request the integrity token with.</param>
        /// <returns>
        /// A <see cref="PlayAsyncOperation{StandardIntegrityToken, StandardIntegrityErrorCode}"/> that returns
        /// <see cref="StandardIntegrityToken"/> on successful callback or
        /// <see cref="StandardIntegrityErrorCode"/> on failure callback.
        /// </returns>
        /// <remarks>
        /// DEPRECATED: Please use <see cref="StandardIntegrityManagerV2.StandardIntegrityTokenProvider.Request(StandardIntegrityTokenRequest)"/> instead.
        ///
        /// <para>This must be called only after <see cref="StandardIntegrityManager.PrepareIntegrityToken"/>
        /// completes.</para>
        /// </remarks>
        public PlayAsyncOperation<StandardIntegrityToken, StandardIntegrityErrorCode> Request(
            StandardIntegrityTokenRequest request)
        {
            var operation = new StandardIntegrityAsyncOperation<StandardIntegrityToken>();

            using (var standardIntegrityTokenRequestClass =
                   new AndroidJavaClass(StandardIntegrityTokenRequestClassName))
            using (var standardIntegrityTokenRequestBuilder =
                   standardIntegrityTokenRequestClass.CallStatic<AndroidJavaObject>("builder"))
            using (var javaVerdictOptOut = new AndroidJavaObject("java.util.HashSet"))
            {
                standardIntegrityTokenRequestBuilder.Call<AndroidJavaObject>("setRequestHash",
                    request.RequestHash);
                foreach (int verdict in request.VerdictOptOut)
                {
                    using (AndroidJavaObject javaInt = new AndroidJavaObject("java.lang.Integer", verdict))
                    {
                        javaVerdictOptOut.Call<bool>("add", javaInt);
                    }
                }
                standardIntegrityTokenRequestBuilder.Call<AndroidJavaObject>("setVerdictOptOut",
                    javaVerdictOptOut);
                var javaStandardIntegrityTokenRequest =
                    standardIntegrityTokenRequestBuilder.Call<AndroidJavaObject>("build");
                var standardIntegrityTokenTask =
                    _playCoreStandardIntegrityTokenProvider.Request(javaStandardIntegrityTokenRequest);

                standardIntegrityTokenTask.RegisterOnSuccessCallback(tokenResponse =>
                {
                    operation.SetResult(
                        new StandardIntegrityToken(tokenResponse));
                    standardIntegrityTokenTask.Dispose();
                    javaStandardIntegrityTokenRequest.Dispose();
                });
                standardIntegrityTokenTask.RegisterOnFailureCallback((reason, errorCode) =>
                {
                    operation.SetError(PlayCoreTranslator.TranslatePlayCoreStandardIntegrityErrorCode(errorCode));
                    standardIntegrityTokenTask.Dispose();
                    javaStandardIntegrityTokenRequest.Dispose();
                });
            }

            return operation;
        }
    }
}