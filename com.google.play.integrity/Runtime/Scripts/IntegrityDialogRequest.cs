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
using UnityEngine;

namespace Google.Play.Integrity
{
    /// <summary>
    /// Represents a request to show a Play Integrity API dialog in Classic mode.
    /// </summary>
    public class IntegrityDialogRequest
    {
        /// <summary>
        /// The Integrity token response that necessitates showing the dialog.
        /// Will be null if the request was built with an error.
        /// </summary>
        public IntegrityTokenResponse TokenResponse { get; private set; }

        /// <summary>
        /// The Integrity error that the dialog is meant to help the user resolve.
        /// Will be null if the request was built with a token response.
        /// </summary>
        public IntegrityServiceError IntegrityError { get; private set; }

        /// <summary>
        /// The Android Activity context used to display the dialog.
        /// </summary>
        public AndroidJavaObject Activity { get; private set; }

        /// <summary>
        /// Determines which Integrity Dialog type should be shown. See
        /// https://developer.android.com/google/play/integrity/reference/com/google/android/play/core/integrity/model/IntegrityDialogTypeCode
        /// for the supported types.
        /// </summary>
        public int DialogType { get; private set; }

        /// <summary>
        /// Constructs an IntegrityDialogRequest using a successful token response.
        /// </summary>
        /// <param name="tokenResponse">The successful response from the Integrity API.</param>
        /// <param name="activity">The Android Activity context used to display the dialog.</param>
        /// <param name="dialogType">The type code of the dialog to show. See
        /// https://developer.android.com/google/play/integrity/reference/com/google/android/play/core/integrity/model/IntegrityDialogTypeCode
        /// for the supported types.</param>
        public IntegrityDialogRequest(IntegrityTokenResponse tokenResponse, AndroidJavaObject activity, int dialogType) 
            : this(activity, dialogType)
        {
            if (tokenResponse == null)
            {
                throw new ArgumentNullException(nameof(tokenResponse));
            }

            TokenResponse = tokenResponse;
        }

        /// <summary>
        /// Constructs an IntegrityDialogRequest using a service error.
        /// </summary>
        /// <param name="integrityError">The error that the dialog is meant to help the user resolve.</param>
        /// <param name="activity">The Android Activity context used to display the dialog.</param>
        /// <param name="dialogType">The type code of the dialog to show. See
        /// https://developer.android.com/google/play/integrity/reference/com/google/android/play/core/integrity/model/IntegrityDialogTypeCode
        /// for the supported types.</param>
        public IntegrityDialogRequest(IntegrityServiceError integrityError, AndroidJavaObject activity, int dialogType) 
            : this(activity, dialogType)
        {
            if (integrityError == null)
            {
                throw new ArgumentNullException(nameof(integrityError));
            }

            IntegrityError = integrityError;
        }

        private IntegrityDialogRequest(AndroidJavaObject activity, int dialogType)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            Activity = activity;
            DialogType = dialogType;
        }
    }
}
