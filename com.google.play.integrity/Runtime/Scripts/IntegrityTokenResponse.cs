// Copyright 2021 Google LLC
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

using Google.Play.Common;
using Google.Play.Core.Internal;
using Google.Play.Integrity.Internal;
using UnityEngine;

namespace Google.Play.Integrity
{
    /// <summary>
    /// Response for <see cref="IntegrityManager.RequestIntegrityToken"/>.
    /// </summary>
    public class IntegrityTokenResponse
    {
        /// <summary>
        /// A token which contains the Base64 encoded response for the integrity related enquiries. The response
        /// consists of a JSON payload that is signed and encrypted as a nested JSON Web Token (JWT).
        /// </summary>
        public string Token { get; private set; }

        private readonly PlayCoreIntegrityTokenResponse _playCoreIntegrityTokenResponse;

        internal IntegrityTokenResponse(AndroidJavaObject tokenResponse)
        {
            _playCoreIntegrityTokenResponse = new PlayCoreIntegrityTokenResponse(tokenResponse);
            var javaTokenString = tokenResponse.Call<AndroidJavaObject>("token");
            Token = PlayCoreHelper.ConvertJavaString(javaTokenString);
        }

        /// <summary>
        /// Displays a dialog to the user. This method can only be called once per
        /// Integrity API response.
        ///
        /// <param name="typeCode">determines which Integrity Dialog type should be shown. See
        /// https://developer.android.com/google/play/integrity/reference/com/google/android/play/core/integrity/model/IntegrityDialogTypeCode
        /// for the supported types.</param>
        /// </summary>
        ///
        /// <returns>
        /// A <see cref="PlayAsyncOperation{TResult,TError}"/> that returns
        /// <see cref="IntegrityDialogResponseCode"/> on successful callback or
        /// <see cref="IntegrityErrorCode"/> on failure callback.
        /// </returns>
        public PlayAsyncOperation<IntegrityDialogResponseCode, IntegrityErrorCode> ShowDialog(int typeCode)
        {
            var operation = new IntegrityAsyncOperation<IntegrityDialogResponseCode>();
            var showDialogTask = _playCoreIntegrityTokenResponse.ShowDialog(typeCode);
            showDialogTask.RegisterOnSuccessCallback(integrityDialogResponseCode =>
            {
                operation.SetResult(
                    PlayCoreTranslator.TranslatePlayCoreIntegrityDialogResponseCode(integrityDialogResponseCode));
                showDialogTask.Dispose();
                _playCoreIntegrityTokenResponse.Dispose();
            });
            showDialogTask.RegisterOnFailureCallback((reason, errorCode) =>
            {
                operation.SetError(PlayCoreTranslator.TranslatePlayCoreErrorCode(errorCode));
                showDialogTask.Dispose();
                _playCoreIntegrityTokenResponse.Dispose();
            });
            return operation;
        }
    }
}