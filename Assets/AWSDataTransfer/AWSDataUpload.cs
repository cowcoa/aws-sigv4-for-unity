using System.Collections.Generic;

using AWSSignatureV4.Signers;

namespace AWSUtilities
{
    public class AWSDataUpload
    {
        const string Region = "us-east-1";
        const string Bucket = "picvox-image-upload-v2-us-east-1";

        /// <summary>
        /// Concatenate the URI used to upload the file
        /// </summary>
        /// <param name="userName">Player user name</param>
        /// <param name="envType">Upload file environment</param>
        /// <param name="uploadPath">Upload file save path</param>
        /// <param name="accelerate">Whether to enable upload acceleration</param>
        /// <returns></returns>
        public static System.Uri SpliceUploadUri(string userName, EnvType envType, string uploadPath, bool accelerate = true)
        {
            string bucketName = string.Format("{0}-{1}", Bucket, StringUtil.EnvTypeToString(envType));
            string objectKey = string.Format("{0}/{1}", userName, uploadPath);

            // Construct a virtual hosted style address with the bucket name part of the host address,
            // placing the region into the url if we're not using us-east-1.
            string regionUrlPart = string.Empty;
            if (accelerate)
            {
                regionUrlPart = "-accelerate";
            }
            else 
            {
                if (!Region.Equals("us-east-1", System.StringComparison.OrdinalIgnoreCase))
                {
                    regionUrlPart = string.Format("-{0}", Region);
                }
            }

            string endpointUri = string.Format("https://{0}.s3{1}.amazonaws.com/{2}",
                                               bucketName,
                                               regionUrlPart,
                                               objectKey);

            return new System.Uri(endpointUri);
        }

        public static void FillHeaders(ref Dictionary<string, string> headers, 
                                       string levelId,
                                       System.Uri requestUri, byte[] uploadContent,
                                       string accessKeyId, string secretAccessKey, string sessionToken)
        {
            headers.Add("Content-Length", uploadContent.Length.ToString());
            headers.Add("Content-Type", "image/jpeg");

            // Add levelid metadata
            headers.Add("x-amz-meta-levelid", levelId);

            var contentHash = AWS4SignerBase.CanonicalRequestHashAlgorithm.ComputeHash(uploadContent);
            var contentHashString = AWS4SignerBase.ToHexString(contentHash, true);
            headers.Add(AWS4SignerBase.X_Amz_Content_SHA256, contentHashString);

            string requestDate = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss.000Z");
            headers.Add("Date", requestDate);

            // This header must be added before sign
            headers.Add("x-amz-security-token", sessionToken);
            var signer = new AWS4SignerForAuthorizationHeader
            {
                EndpointUri = requestUri,
                HttpMethod = "PUT",
                Service = "s3",
                Region = Region
            };
            var authorization = signer.ComputeSignature(headers,
                                                        "",   // no query parameters
                                                        contentHashString,
                                                        accessKeyId,
                                                        secretAccessKey);
            headers.Add("Authorization", authorization);
        }
    }
}
