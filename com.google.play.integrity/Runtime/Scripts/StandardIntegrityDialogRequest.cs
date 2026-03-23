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
    /// Represents a request to show a Play Integrity API dialog in Standard mode.
    /// </summary>
    public class StandardIntegrityDialogRequest
    {
        /// <summary>
        /// The Standard Integrity token response that necessitates showing the dialog.
        /// Will be null if the request was built with an error.
        /// </summary>
        public StandardIntegrityToken StandardIntegrityToken { get; private set; }

        /// <summary>
        /// The Standard Integrity error that the dialog is meant to help the user resolve.
        /// Will be null if the request was built with a token response.
        /// </summary>
        public StandardIntegrityError StandardIntegrityError { get; private set; }

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
        /// Constructs a StandardIntegrityDialogRequest using a successful token response.
        /// </summary>
        /// <param name="token">The successful response from the Integrity API.</param>
        /// <param name="activity">The Android Activity context used to display the dialog.</param>
        /// <param name="dialogType">The type code of the dialog to show. See
        /// https://developer.android.com/google/play/integrity/reference/com/google/android/play/core/integrity/model/IntegrityDialogTypeCode
        /// for the supported types.</param>
        public StandardIntegrityDialogRequest(StandardIntegrityToken token, AndroidJavaObject activity, int dialogType) 
            : this(activity, dialogType)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            StandardIntegrityToken = token;
        }

        /// <summary>
        /// Constructs a StandardIntegrityDialogRequest using a service error.
        /// </summary>
        /// <param name="standardIntegrityError">The error that the dialog is meant to help the user resolve.</param>
        /// <param name="activity">The Android Activity context used to display the dialog.</param>
        /// <param name="dialogType">The type code of the dialog to show. See
        /// https://developer.android.com/google/play/integrity/reference/com/google/android/play/core/integrity/model/IntegrityDialogTypeCode
        /// for the supported types.</param>
        public StandardIntegrityDialogRequest(StandardIntegrityError standardIntegrityError, AndroidJavaObject activity, int dialogType) 
            : this(activity, dialogType)
        {
            if (standardIntegrityError == null)
            {
                throw new ArgumentNullException(nameof(standardIntegrityError));
            }

            StandardIntegrityError = standardIntegrityError;
        }

        private StandardIntegrityDialogRequest(AndroidJavaObject activity, int dialogType)
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
