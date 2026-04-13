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
    /// Replaces the V1 IntegrityManager to provide enhanced error handling and remediation dialog support.
    /// </summary>
    public class IntegrityManagerV2
    {
        private const string IntegrityTokenRequestClassName = PlayCoreConstants.IntegrityPackagePrefix + "IntegrityTokenRequest";
        private const string IntegrityDialogRequestClassName = PlayCoreConstants.IntegrityPackagePrefix + "IntegrityDialogRequest";
        private const string IntegrityResponseTokenResponseClassName = PlayCoreConstants.IntegrityPackagePrefix + "IntegrityDialogRequest$IntegrityResponse$TokenResponse";
        private const string IntegrityResponseExceptionDetailsClassName = PlayCoreConstants.IntegrityPackagePrefix + "IntegrityDialogRequest$IntegrityResponse$ExceptionDetails";

        private readonly PlayCoreIntegrityManager _playCoreIntegrityManager;

        public IntegrityManagerV2()
        {
            _playCoreIntegrityManager = new PlayCoreIntegrityManager();
        }

        /// <summary>
        /// Starts a PlayAsyncOperation to generate a token for integrity-related enquiries, and provides the token as
        /// its result.
        ///
        /// <p>The JSON payload is signed and encrypted as a nested JSON Web Token (JWT), that is
        /// <a href="https://tools.ietf.org/html/rfc7516">JWE</a> of
        /// <a href="https://tools.ietf.org/html/rfc7515">JWS</a>.
        ///
        /// <p>JWE uses <a href="https://tools.ietf.org/html/rfc7518#section-4.4">A256KW</a> as a key wrapping
        /// algorithm and <a href="https://tools.ietf.org/html/rfc7518#section-5.3">A256GCM</a> as a content encryption
        /// algorithm. JWS uses <a href="https://tools.ietf.org/html/rfc7518#section-3.4">ES256</a> as a signing
        /// algorithm.
        ///
        /// <p>All decryption and verification should be done within a secure server environment. Do not decrypt or
        /// verify the received token from within the client app. In particular, never expose any decryption keys to the
        /// client app.
        ///
        /// <p>See https://developer.android.com/google/play/integrity/verdict#token-format.
        /// </summary>
        /// <returns>
        /// A <see cref="PlayAsyncOperation{IntegrityTokenResponse, IntegrityServiceError}"/> that returns
        /// <see cref="IntegrityTokenResponse"/> on successful callback or <see cref="IntegrityServiceError"/> on failure
        /// callback.
        /// </returns>
        public PlayAsyncOperation<IntegrityTokenResponse, IntegrityServiceError> RequestIntegrityToken(IntegrityTokenRequest integrityTokenRequest)
        {
            var operation = new IntegrityAsyncOperationV2<IntegrityTokenResponse>();

            using (var integrityTokenRequestClass = new AndroidJavaClass(IntegrityTokenRequestClassName))
            using (var integrityTokenRequestBuilder = integrityTokenRequestClass.CallStatic<AndroidJavaObject>("builder"))
            {
                // FIXED: Call<AndroidJavaObject> to match signature, chain .Dispose() to prevent leak
                integrityTokenRequestBuilder.Call<AndroidJavaObject>("setNonce", integrityTokenRequest.Nonce).Dispose();

                if (integrityTokenRequest.CloudProjectNumber.HasValue)
                {
                    integrityTokenRequestBuilder.Call<AndroidJavaObject>("setCloudProjectNumber", integrityTokenRequest.CloudProjectNumber.Value).Dispose();
                }

                var javaIntegrityTokenRequest = integrityTokenRequestBuilder.Call<AndroidJavaObject>("build");
                var requestIntegrityTokenTask = _playCoreIntegrityManager.RequestIntegrityToken(javaIntegrityTokenRequest);

                requestIntegrityTokenTask.RegisterOnSuccessCallback(javaTokenResponse =>
                {
                    operation.SetResult(new IntegrityTokenResponse(javaTokenResponse));
                    requestIntegrityTokenTask.Dispose();
                    javaIntegrityTokenRequest.Dispose();
                });

                requestIntegrityTokenTask.RegisterOnFailureCallback((AndroidJavaObject rawException) =>
                {
                    operation.SetError(new IntegrityServiceError(rawException));
                    requestIntegrityTokenTask.Dispose();
                    javaIntegrityTokenRequest.Dispose();
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
        /// <see cref="IntegrityServiceError"/> on failure callback.
        /// </returns>
        public PlayAsyncOperation<IntegrityDialogResponseCode, IntegrityServiceError> ShowDialog(IntegrityDialogRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var operation = new IntegrityAsyncOperationV2<IntegrityDialogResponseCode>();

            using (var dialogRequestClass = new AndroidJavaClass(IntegrityDialogRequestClassName))
            using (var builder = dialogRequestClass.CallStatic<AndroidJavaObject>("builder"))
            {
                builder.Call<AndroidJavaObject>("setActivity", request.Activity).Dispose();
                builder.Call<AndroidJavaObject>("setTypeCode", request.DialogType).Dispose();

                AndroidJavaObject javaIntegrityResponse = null;

                if (request.TokenResponse != null)
                {
                    javaIntegrityResponse = new AndroidJavaObject(
                        IntegrityResponseTokenResponseClassName,
                        request.TokenResponse.GetJavaTokenResponse());
                }
                else if (request.IntegrityError != null)
                {
                    javaIntegrityResponse = new AndroidJavaObject(
                        IntegrityResponseExceptionDetailsClassName,
                        request.IntegrityError.GetJavaException());
                }
                else
                {
                    throw new ArgumentException("IntegrityDialogRequest must contain either a valid TokenResponse or an IntegrityError.");
                }

                builder.Call<AndroidJavaObject>("setIntegrityResponse", javaIntegrityResponse).Dispose();
                javaIntegrityResponse.Dispose();

                var javaDialogRequest = builder.Call<AndroidJavaObject>("build");
                var showDialogTask = _playCoreIntegrityManager.ShowDialog(javaDialogRequest);

                showDialogTask.RegisterOnSuccessCallback(javaResponseObject =>
                {
                    operation.SetResult(PlayCoreTranslator.TranslatePlayCoreIntegrityDialogResponseCode(javaResponseObject));
                    showDialogTask.Dispose();
                    javaDialogRequest.Dispose();
                });

                showDialogTask.RegisterOnFailureCallback((AndroidJavaObject rawException) =>
                {
                    operation.SetError(new IntegrityServiceError(rawException));
                    showDialogTask.Dispose();
                    javaDialogRequest.Dispose();
                });
            }

            return operation;
        }
    }
}