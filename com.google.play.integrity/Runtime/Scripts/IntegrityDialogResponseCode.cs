// Copyright 2024 Google LLC
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

namespace Google.Play.Integrity
{
    /// <summary>
    /// Status returned when showing the integrity dialog
    /// </summary>
    public enum IntegrityDialogResponseCode
    {
        /// <summary>
        /// The Integrity Dialog is unavailable.
        /// </summary>
        DialogUnavailable = 0,

        /// <summary>
        /// An error occurred when trying to show the Integrity Dialog.
        /// </summary>
        DialogFailed = 1,

        /// <summary>
        /// The user was shown the Integrity Dialog, but did not interact with it.
        /// </summary>
        DialogCancelled = 2,

        /// <summary>
        /// The user was shown the Integrity Dialog, and successfully interacted with it.
        /// </summary>
        DialogSuccessful = 3,
    }
}
