// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrustAllCertificatePolicy.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the TrustAllCertificatePolicy type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.Translator.ApplicationFeatures
{
    using System.Net;
    using System.Security.Cryptography.X509Certificates;

    public class TrustAllCertificatePolicy : ICertificatePolicy
    {
        public bool CheckValidationResult(ServicePoint sp,
                                          X509Certificate cert,
                                          WebRequest req,
                                          int problem)
        {
            return true;
        }
    }
}