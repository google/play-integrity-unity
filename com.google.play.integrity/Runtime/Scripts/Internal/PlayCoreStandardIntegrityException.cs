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
    /// Internal implementation of StandardIntegrityException that wraps Play Core's StandardIntegrityException.
    /// </summary>
    internal sealed class PlayCoreStandardIntegrityException : IDisposable
    {
        private readonly AndroidJavaObject _javaStandardIntegrityException;
        private bool _disposed;

        // Backing fields to cache expensive JNI calls
        private int? _errorCode;
        private bool? _isRemediable;

        internal PlayCoreStandardIntegrityException(AndroidJavaObject javaStandardIntegrityException)
        {
            if (javaStandardIntegrityException == null)
            {
                throw new ArgumentNullException(nameof(javaStandardIntegrityException));
            }

            _javaStandardIntegrityException = javaStandardIntegrityException;
        }

        internal AndroidJavaObject GetJavaException()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(PlayCoreStandardIntegrityException));
            return _javaStandardIntegrityException;
        }

        internal int ErrorCode
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException(nameof(PlayCoreStandardIntegrityException));

                if (!_errorCode.HasValue)
                {
                    _errorCode = _javaStandardIntegrityException.Call<int>("getErrorCode");
                }
                return _errorCode.Value;
            }
        }

        internal bool IsRemediable
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException(nameof(PlayCoreStandardIntegrityException));

                if (!_isRemediable.HasValue)
                {
                    _isRemediable = _javaStandardIntegrityException.Call<bool>("isRemediable");
                }
                return _isRemediable.Value;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _javaStandardIntegrityException.Dispose();
            _disposed = true;
        }
    }
}
