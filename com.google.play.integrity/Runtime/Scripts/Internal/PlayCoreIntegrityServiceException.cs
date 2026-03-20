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

namespace Google.Play.Integrity.Internal
{
    /// <summary>
    /// Internal implementation of IntegrityServiceException that wraps Play Core's IntegrityServiceException.
    /// </summary>
    internal sealed class PlayCoreIntegrityServiceException : IDisposable
    {
        private readonly AndroidJavaObject _javaIntegrityServiceException;
        private bool _disposed;

        // Backing fields to cache expensive JNI calls
        private int? _errorCode;
        private bool? _isRemediable;

        internal PlayCoreIntegrityServiceException(AndroidJavaObject javaIntegrityServiceException)
        {
            if (javaIntegrityServiceException == null)
            {
                throw new ArgumentNullException(nameof(javaIntegrityServiceException));
            }

            _javaIntegrityServiceException = javaIntegrityServiceException;
        }

        internal int ErrorCode
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException(nameof(PlayCoreIntegrityServiceException));

                if (!_errorCode.HasValue)
                {
                    _errorCode = _javaIntegrityServiceException.Call<int>("getErrorCode");
                }
                return _errorCode.Value;
            }
        }

        internal bool IsRemediable
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException(nameof(PlayCoreIntegrityServiceException));

                if (!_isRemediable.HasValue)
                {
                    _isRemediable = _javaIntegrityServiceException.Call<bool>("isRemediable");
                }
                return _isRemediable.Value;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _javaIntegrityServiceException.Dispose();
            _disposed = true;
        }
    }
}
