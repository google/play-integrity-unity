// Copyright 2026 Google LLC
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
    /// Manages requests for integrity information and dialogs.
    /// </summary>
    public class StandardIntegrityManagerV2
    {
        private readonly PlayCoreStandardIntegrityManager _playCoreStandardIntegrityManager;

        private const string PrepareIntegrityTokenRequestClassName =
            PlayCoreConstants.IntegrityPackagePrefix + "StandardIntegrityManager$PrepareIntegrityTokenRequest";
        private const string StandardIntegrityDialogRequestClassName = 
            PlayCoreConstants.IntegrityPackagePrefix + "StandardIntegrityManager$StandardIntegrityDialogRequest";
        private const string StandardIntegrityResponseTokenResponseClassName = 
            PlayCoreConstants.IntegrityPackagePrefix + "StandardIntegrityManager$StandardIntegrityDialogRequest$StandardIntegrityResponse$TokenResponse";
        private const string StandardIntegrityResponseExceptionDetailsClassName = 
            PlayCoreConstants.IntegrityPackagePrefix + "StandardIntegrityManager$StandardIntegrityDialogRequest$StandardIntegrityResponse$ExceptionDetails";

        /// <summary>
        ///  Constructor.
        /// </summary>
        public StandardIntegrityManagerV2()
        {
            _playCoreStandardIntegrityManager = new PlayCoreStandardIntegrityManager();
        }

        /// <summary>
        /// Prepares the integrity token and makes it available for requesting via
        /// <see cref="StandardIntegrityTokenProvider"/>.
        ///
        /// <para>You can call this method from time to time in order to refresh the resulting
        /// <see cref="StandardIntegrityTokenProvider"/>.</para>
        ///
        /// <para>The API makes a call to Google's servers and hence requires a network connection.</para>
        ///
        /// </summary>
        /// <param name="request">the object to prepare the integrity token with.</param>
        /// <returns>
        /// A <see cref="PlayAsyncOperation{StandardIntegrityManagerV2.StandardIntegrityTokenProvider, StandardIntegrityError}"/> that returns
        /// <see cref="StandardIntegrityTokenProvider"/> on successful callback or
        /// <see cref="StandardIntegrityError"/> on failure callback.
        /// </returns>
        public PlayAsyncOperation<StandardIntegrityManagerV2.StandardIntegrityTokenProvider, StandardIntegrityError> PrepareIntegrityToken(
            PrepareIntegrityTokenRequest request)
        {
            var operation = new StandardIntegrityAsyncOperationV2<StandardIntegrityManagerV2.StandardIntegrityTokenProvider>();

            using (var prepareIntegrityTokenRequestClass = new AndroidJavaClass(PrepareIntegrityTokenRequestClassName))
            using (var prepareIntegrityTokenRequestBuilder =
                   prepareIntegrityTokenRequestClass.CallStatic<AndroidJavaObject>("builder"))
            {
                prepareIntegrityTokenRequestBuilder.Call<AndroidJavaObject>("setCloudProjectNumber",
                    request.CloudProjectNumber).Dispose();
                var javaPrepareIntegrityTokenRequest =
                    prepareIntegrityTokenRequestBuilder.Call<AndroidJavaObject>("build");

                var prepareIntegrityTokenTask =
                    _playCoreStandardIntegrityManager.PrepareIntegrityToken(javaPrepareIntegrityTokenRequest);

                prepareIntegrityTokenTask.RegisterOnSuccessCallback(tokenProvider =>
                {
                    operation.SetResult(
                        new StandardIntegrityTokenProvider(new PlayCoreStandardIntegrityTokenProvider(tokenProvider)));
                    prepareIntegrityTokenTask.Dispose();
                    javaPrepareIntegrityTokenRequest.Dispose();
                });

                prepareIntegrityTokenTask.RegisterOnFailureCallback((AndroidJavaObject rawException) =>
                {
                    operation.SetError(new StandardIntegrityError(rawException));
                    prepareIntegrityTokenTask.Dispose();
                    javaPrepareIntegrityTokenRequest.Dispose();
                });
            }

            return operation;
        }

        /// <summary>
        /// Displays a dialog to the user. This method can only be called once per
        /// Integrity API response (Token or Error).
        /// </summary>
        /// <param name="request">Contains all the information required to show the dialog.</param>
        /// <returns>
        /// A <see cref="PlayAsyncOperation{TResult,TError}"/> that returns
        /// <see cref="IntegrityDialogResponseCode"/> on successful callback or
        /// <see cref="StandardIntegrityError"/> on failure callback.
        /// </returns>
        public PlayAsyncOperation<IntegrityDialogResponseCode, StandardIntegrityError> ShowDialog(
            StandardIntegrityDialogRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var operation = new StandardIntegrityAsyncOperationV2<IntegrityDialogResponseCode>();

            using (var dialogRequestClass = new AndroidJavaClass(StandardIntegrityDialogRequestClassName))
            using (var builder = dialogRequestClass.CallStatic<AndroidJavaObject>("builder"))
            {
                builder.Call<AndroidJavaObject>("setActivity", request.Activity).Dispose();
                builder.Call<AndroidJavaObject>("setTypeCode", request.DialogType).Dispose();

                AndroidJavaObject javaIntegrityResponse;

                if (request.StandardIntegrityToken != null)
                {
                    javaIntegrityResponse = new AndroidJavaObject(
                        StandardIntegrityResponseTokenResponseClassName, 
                        request.StandardIntegrityToken.GetJavaTokenResponse());
                }
                else if (request.StandardIntegrityError != null)
                {
                    javaIntegrityResponse = new AndroidJavaObject(
                        StandardIntegrityResponseExceptionDetailsClassName, 
                        request.StandardIntegrityError.GetJavaException());
                }
                else
                {
                    throw new ArgumentException("StandardIntegrityDialogRequest must contain either a valid StandardIntegrityToken or a StandardIntegrityError.");
                }

                builder.Call<AndroidJavaObject>("setStandardIntegrityResponse", javaIntegrityResponse).Dispose();
                javaIntegrityResponse.Dispose();

                var javaDialogRequest = builder.Call<AndroidJavaObject>("build");
                var showDialogTask = _playCoreStandardIntegrityManager.ShowDialog(javaDialogRequest);

                showDialogTask.RegisterOnSuccessCallback(dialogResponseCode =>
                {
                    operation.SetResult(PlayCoreTranslator.TranslatePlayCoreIntegrityDialogResponseCode(dialogResponseCode));
                    showDialogTask.Dispose();
                    javaDialogRequest.Dispose();
                });

                showDialogTask.RegisterOnFailureCallback((AndroidJavaObject rawException) =>
                {
                    operation.SetError(new StandardIntegrityError(rawException));
                    showDialogTask.Dispose();
                    javaDialogRequest.Dispose();
                });
            }

            return operation;
        }

        /// <summary>
        /// Standard integrity token provider.
        ///
        /// <para>Response of <see cref="StandardIntegrityManagerV2.PrepareIntegrityToken"/> call.</para>
        /// </summary>
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
            ///
            /// <para> This must be called only after <see cref="StandardIntegrityManagerV2.PrepareIntegrityToken"/>
            /// completes.</para>
            ///
            /// </summary>
            /// <param name="request">the object to request integrity token with.</param>
            /// <returns>
            /// A <see cref="PlayAsyncOperation{StandardIntegrityToken, StandardIntegrityError}"/> that returns
            /// <see cref="StandardIntegrityToken"/> on successful callback or
            /// <see cref="StandardIntegrityError"/> on failure callback.
            /// </returns>
            public PlayAsyncOperation<StandardIntegrityToken, StandardIntegrityError> Request(
                StandardIntegrityTokenRequest request)
            {
                var operation = new StandardIntegrityAsyncOperationV2<StandardIntegrityToken>();

                using (var standardIntegrityTokenRequestClass =
                       new AndroidJavaClass(StandardIntegrityTokenRequestClassName))
                using (var standardIntegrityTokenRequestBuilder =
                       standardIntegrityTokenRequestClass.CallStatic<AndroidJavaObject>("builder"))
                using (var javaVerdictOptOut = new AndroidJavaObject("java.util.HashSet"))
                {
                    standardIntegrityTokenRequestBuilder.Call<AndroidJavaObject>("setRequestHash",
                        request.RequestHash).Dispose();

                    if (request.VerdictOptOut != null)
                    {
                        foreach (int verdict in request.VerdictOptOut)
                        {
                            using (AndroidJavaObject javaInt = new AndroidJavaObject("java.lang.Integer", verdict))
                            {
                                javaVerdictOptOut.Call<bool>("add", javaInt);
                            }
                        }
                    }
                    standardIntegrityTokenRequestBuilder.Call<AndroidJavaObject>("setVerdictOptOut",
                        javaVerdictOptOut).Dispose();

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

                    standardIntegrityTokenTask.RegisterOnFailureCallback((AndroidJavaObject rawException) =>
                    {
                        operation.SetError(new StandardIntegrityError(rawException));
                        standardIntegrityTokenTask.Dispose();
                        javaStandardIntegrityTokenRequest.Dispose();
                    });
                }

                return operation;
            }
        }
    }
}