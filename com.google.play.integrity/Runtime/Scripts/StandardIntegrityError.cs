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
using Google.Play.Integrity.Internal;

namespace Google.Play.Integrity
{
    /// <summary>
    /// Represents an error returned by the Play Integrity API.
    /// This object holds native resources and must be disposed when no longer needed.
    /// </summary>
    public class StandardIntegrityError : IDisposable
    {
        /// <summary>
        /// The error code associated with the failure.
        /// </summary>
        public StandardIntegrityErrorCode ErrorCode { get; private set; }

        /// <summary>
        /// Indicates whether the error is remediable and can be resolved by user action.
        /// </summary>
        public bool IsRemediable { get; private set; }

        private readonly PlayCoreStandardIntegrityException _internalException;
        private bool _disposed;

        internal StandardIntegrityError(AndroidJavaObject javaException)
        {
            if (javaException == null)
            {
                throw new ArgumentNullException(nameof(javaException));
            }

            _internalException = new PlayCoreStandardIntegrityException(javaException);

            try
            {
                ErrorCode = PlayCoreTranslator.TranslatePlayCoreStandardIntegrityErrorCode(_internalException.ErrorCode);
            }
            catch (AndroidJavaException)
            {
                ErrorCode = StandardIntegrityErrorCode.InternalError;
            }

            try
            {
                IsRemediable = _internalException.IsRemediable;
            }
            catch (AndroidJavaException)
            {
                IsRemediable = false;
            }
        }

        internal AndroidJavaObject GetJavaException()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(StandardIntegrityError));
            return _internalException.GetJavaException();
        }

        /// <summary>
        /// Disposes the underlying native Android resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _internalException.Dispose();
            _disposed = true;
        }
    }
}
