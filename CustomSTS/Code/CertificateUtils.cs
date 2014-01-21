using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace CustomSTS.Code
{
    public class CertificateUtil
    {
        static Dictionary<string, X509Certificate2> _certificates = new Dictionary<string, X509Certificate2>();

        public static X509Certificate2 GetCertificateFromStore(string Subject)
        {
            if (!_certificates.ContainsKey(Subject))
            {
                X509Store store = new X509Store(StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2 cert = null;

                foreach (X509Certificate2 mCert in store.Certificates)
                {
                    if (mCert.Subject.Equals(Subject))
                    {
                        cert = mCert;
                        break;
                    }
                }

                // TODO: Validate that we have permissions for the private key

                if (cert == null) throw new Exception(String.Format("No certificate was found in users certificate store with subject name: {0}", Subject));

                _certificates[Subject] = cert;
            }
            return _certificates[Subject];
        }

        public static X509Certificate2 GetCertificateFromAssembly(string CertResourceName, string CertPwd)
        {
            if (!_certificates.ContainsKey(CertResourceName))
            {
                foreach (string name in typeof(CertificateUtil).Assembly.GetManifestResourceNames())
                    if (name.EndsWith(CertResourceName))
                    {
                        Stream certificateStream = typeof(CertificateUtil).Assembly.GetManifestResourceStream(name);

                        using (BinaryReader br = new BinaryReader(certificateStream))
                        {
                            _certificates[CertResourceName] = new X509Certificate2(br.ReadBytes((int)certificateStream.Length), CertPwd, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
                        }
                    }
            }

            if (_certificates.ContainsKey(CertResourceName))
            {
                return _certificates[CertResourceName];
            }

            throw new Exception("No certificate was found in application's resources");
        }
    }
}