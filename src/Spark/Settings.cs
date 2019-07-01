using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.Jwt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using Spark.Authentication;

namespace Spark
{
    public class JwtCert
    {
        public string Type { get; set; }
        public string Issuer { get; set; }
        public int? TokenTtl { get; set; }
        public IIssuerSecurityKeyProvider Provider { get; set; }
    }

    public static class Settings
    {
        private static Dictionary<string, JwtCert> jwtCertDict = new Dictionary<string, JwtCert>();

        static Settings()
        {
            ReadJwtCertConfig();
        }

        public static string Version
        {
            get
            {
                var asm = Assembly.GetExecutingAssembly();
                FileVersionInfo version = FileVersionInfo.GetVersionInfo(asm.Location);
                return String.Format("{0}.{1}", version.ProductMajorPart, version.ProductMinorPart);
            }
        }
        public static bool UseS3
        {
            get
            {
                try
                {
                    var useS3 = GetRequiredKey("FHIR_USE_S3");
                    return useS3 == "true";
                }
                catch
                {
                    return false;
                }
            }
        }

        public static int MaxBinarySize
        {
            get
            {
                try
                {
                    int max = Convert.ToInt16(GetRequiredKey("MaxBinarySize"));
                    if (max == 0) max = Int16.MaxValue;
                    return max;
                }
                catch
                {
                    return Int16.MaxValue;
                }
            }
        }

        public static string MongoUrl
        {
            get { return GetRequiredKey("MONGOLAB_URI"); }
        }

        public static string AwsAccessKey
        {
            get { return GetRequiredKey("AWSAccessKey"); }
        }

        public static string AwsSecretKey
        {
            get { return GetRequiredKey("AWSSecretKey"); }
        }

        public static string AwsBucketName
        {
            get { return GetRequiredKey("AWSBucketName"); }
        }

        public static string CertPath {
            get { return GetRequiredKey("JwtCertsConfig"); }
        }

        public static SecurityKey[] JwtKeys {
            get {
                ReadJwtCertConfig();
                return jwtCertDict
                    .Select(e => e.Value.Provider)
                    .SelectMany(e => e.SecurityKeys)
                    .ToArray();
            }
        }

        public static string[] JwtKeyIssuers {
            get {
                ReadJwtCertConfig();
                return jwtCertDict
                    .Select(e => e.Value.Issuer)
                    .ToArray();
            }
        }

        public static IIssuerSecurityKeyProvider[] JwtKeyProviders {
            get
            {
                ReadJwtCertConfig();
                return jwtCertDict
                    .Select(e => e.Value.Provider)
                    .ToArray();
            }
        }

        public static IDictionary<string, JwtCert> JwtCerts {
            get {
                return jwtCertDict;
            }
        }

        public static Uri Endpoint
        {
            get
            {
                string endpoint = GetRequiredKey("FHIR_ENDPOINT");
                return new Uri(endpoint, UriKind.Absolute);
            }
        }

        public static string AuthorUri
        {
            get
            {
                return Endpoint.Host;
            }
        }

        public static string ExamplesFilePath
        {
            get
            {
                string path = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;

                if (String.IsNullOrEmpty(path))
                {
                    path = ".";
                }

                return Path.Combine(path, "Examples", FhirRelease, "examples.zip");
            }
        }

        public static string FhirRelease
        {
            get { return GetRequiredKey("FhirRelease"); }
        }

        public static bool PermissiveParsing
        {
            get
            {
                if (bool.TryParse(GetRequiredKey("PermissiveParsing"), out bool permissiveParsing))
                    return permissiveParsing;
                // Defaults to true so that we adhere to how it was.
                return true;
            }
        }

        public static long MaximumDecompressedBodySizeInBytes
        {
            get { return long.Parse(GetRequiredKey("MaxDecompressedBodySizeInBytes")); }
        }


        private static string GetRequiredKey(string key)
        {
            string s = ConfigurationManager.AppSettings.Get(key);

            if (string.IsNullOrEmpty(s))
                throw new ArgumentException(string.Format("The configuration variable {0} is missing.", key));

            return s;
        }
        private static void ReadJwtCertConfig()
        {
            var log = SparkAuthenticationEventSource.Log;
            jwtCertDict.Clear();
            if (File.Exists(Settings.CertPath))
            {
                var stringReader = new StreamReader(Settings.CertPath);
                JObject config = JObject.Parse(stringReader.ReadToEnd());
                var basePath = (string)config.SelectToken(".path");
                var certConfigs = config.SelectToken(".certs");
                foreach (var certConfig in certConfigs)
                {
                    switch ((string)certConfig.SelectToken(".type"))
                    {
                        case "":
                        case "rsa":
                            var certName = (string)certConfig.SelectToken(".certName");
                            if (!Path.IsPathRooted(certName))
                                certName = Path.Combine(basePath, certName);
                            jwtCertDict.Add(
                                (string)certConfig.SelectToken(".issuer"),
                                new JwtCert()
                                {
                                    Issuer = (string)certConfig.SelectToken(".issuer"),
                                    Provider = new X509CertificateSecurityKeyProvider(
                                        (string)certConfig.SelectToken(".issuer"),
                                        new X509Certificate2(
                                            certName,
                                            "",
                                            X509KeyStorageFlags.DefaultKeySet
                                        )
                                    ),
                                    TokenTtl = (int?)certConfig.SelectToken(".tokenTtl", false),
                                    Type = (string)certConfig.SelectToken(".type")
                                }
                            );
                            break;
                        default:
                            throw new Exception(string.Format("Unsupported type: {0}", certConfig.SelectToken(".type")));
                    }
                }
            }
            else
                log.NoSuchFile(Settings.CertPath);
        }
    }
}