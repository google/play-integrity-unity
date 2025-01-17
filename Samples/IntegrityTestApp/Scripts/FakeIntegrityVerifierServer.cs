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

using System;
using System.Security.Cryptography;
using UnityEngine;

namespace Google.Play.Integrity.Samples.IntegrityTestApp
{
    /// <summary>
    /// Provides helper functions for generating nonces and decoding tokens.
    /// </summary>
    internal class FakeIntegrityVerifierServer
    {
        /// <summary>
        /// The response of the decryption and verification.
        /// </summary>
        public enum DecryptionResponse { Allow, AllowWithLimits, Deny }

        /// <summary>
        /// Generates the nonce as a random byte array of length numBytes, returns the Base64 representation of this
        /// byte array.
        /// </summary>
        public static string GenerateNonce(int numBytes)
        {
            var byteArray = new byte[numBytes];
            var rngProvider = new RNGCryptoServiceProvider();
            rngProvider.GetBytes(byteArray);
            var nonce = Convert.ToBase64String(byteArray);
            // Convert to URL-safe encoding
            nonce = nonce.Replace("+", "-").Replace("/", "_");
            return nonce;
        }

        /// <summary>
        /// Decrypts and verifies the integrity token.
        /// </summary>
        public static DecryptionResponse DecryptAndVerify(string encryptedToken)
        {
            // After you request an integrity verdict, the Play Integrity API provides an encrypted
            // response token. To obtain the device integrity verdicts, you must decrypt the integrity
            // token on Google's servers.
            // Refer to the documentation for more details:
            // https://developer.android.com/google/play/integrity/standard#decrypt-and
            return DecryptionResponse.Allow;
        }

    }
}