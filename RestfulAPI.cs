using Marel.Models;
using Marel.Models.Enum;
using Marel.MSAUploadGenerator.NextGenAPI.Models;
using RestSharp;
using System;
using System.IO;
using System.Linq;

namespace Marel.MSAUploadGenerator.NextGenAPI
{
    public static class RestfulAPI
    {
        // Rest Client
        private static RestClient Client { get; set; }

        public static void Init()
        {
            // Create the rest client to do the conneciton
            Client = new RestClient(AppSettings.RestUrl);
            Client.AddDefaultHeader("Authorization", $"api-key {AppSettings.ApiKey}");

            // Initialize paths
            if (!Directory.Exists(AppSettings.BasePath))
                Directory.CreateDirectory(Path.Combine(AppSettings.BasePath, AppSettings.BasePath));

            // Create output foler
            if (!Directory.Exists(Path.Combine(AppSettings.BasePath, AppSettings.Outputfolder)))
                Directory.CreateDirectory(Path.Combine(AppSettings.BasePath, AppSettings.Outputfolder));

            // Create output foler
            if (!Directory.Exists(Path.Combine(AppSettings.BasePath, AppSettings.Outputfolder)))
                Directory.CreateDirectory(Path.Combine(AppSettings.BasePath, AppSettings.Outputfolder));
        }

        /// <summary>
        /// Fetch the producer
        /// </summary>
        /// <param name="msaNo"></param>
        /// <returns></returns>
        public static Producer GetProducerOnMsa(string msaNo)
        {
            // Create a new request
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = "/Producers";
            request.AddHeader("Content-Type", "application/json");
            request.AddQueryParameter("msaNumber", msaNo);

            var response = Client.Execute<RESTtResult<Producer>>(request);

            return response.Data.results.FirstOrDefault();
        }

        /// <summary>
        /// Upload the file to MSA Grading
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static UploadSummary Upload(string name, string path)
        {
            var request = new RestRequest();

            request.Method = Method.POST;
            request.Resource = "/grading";
            request.AlwaysMultipartFormData = true;
            request.AddHeader("Content-Type", "multipart/form-data");
            request.AddQueryParameter("json", "");
            request.AddFile(name, path);

            var response = Client.Execute<UploadSummary>(request);

            return response.Data;
        }

        /// <summary>
        /// Upload the file to MSA Grading Sandbox
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static UploadSummary UploadToSandbox(string name, string path)
        {
            var request = new RestRequest();

            request.Method = Method.POST;
            request.Resource = "/grading/sandbox";
            request.AlwaysMultipartFormData = true;
            request.AddHeader("Content-Type", "multipart/form-data");
            request.AddQueryParameter("json", "");
            request.AddFile(name, path);

            var response = Client.Execute<UploadSummary>(request);

            return response.Data;
        }

        /// <summary>
        /// Download the grading data
        /// </summary>
        /// <param name="killDateFrom"></param>
        /// <param name="killDateTo"></param>
        /// <param name="gradeFieldList"></param>
        /// <param name="species"></param>
        /// <param name="sourceId"></param>
        /// <param name="plant"></param>
        /// <param name="vendorProducer"></param>
        /// <param name="ownerProducer"></param>
        /// <param name="lot"></param>
        /// <param name="bodyNo"></param>
        /// <param name="operator"></param>
        /// <param name="page"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static DownloadSummary DownloadMSASummary(
            DateTime? killDateFrom,
            DateTime? killDateTo,
            GradeFieldList gradeFieldList,
            Species species,
            int? sourceId,
            string plant,
            string vendorProducer,
            string ownerProducer,
            string lot,
            int? bodyNo,
            string @operator,
            int? page,
            int? count
            )
        {
            var request = new RestRequest();

            request.Method = Method.GET;
            request.Resource = "/grading/grades";
            request.AlwaysMultipartFormData = true;
            request.AddHeader("accept", "application/json");

            // Required Fields
            request.AddQueryParameter("gradeFieldList", gradeFieldList.ToString());
            request.AddQueryParameter("species", species.ToString());

            // Optional Fields
            if (killDateFrom != null)
                request.AddQueryParameter("killDateFrom", killDateFrom.Value.ToString("yyyyMMdd"));

            if (killDateTo != null)
                request.AddQueryParameter("killDateTo", killDateTo.Value.ToString("yyyyMMdd"));

            if (sourceId != null)
                request.AddQueryParameter("sourceId", sourceId.ToString());

            if (!string.IsNullOrWhiteSpace(lot))
                request.AddQueryParameter("lot", lot);

            if (bodyNo != null)
                request.AddQueryParameter("bodyNo", bodyNo.ToString());

            if (page != null)
                request.AddQueryParameter("page", page.ToString());

            if (count != null)
                request.AddQueryParameter("count", count.ToString());

            if (!string.IsNullOrWhiteSpace(plant))
                request.AddQueryParameter("plant", plant);

            if (!string.IsNullOrWhiteSpace(vendorProducer))
                request.AddQueryParameter("vendorProducer", vendorProducer);

            if (!string.IsNullOrWhiteSpace(ownerProducer))
                request.AddQueryParameter("ownerProducer", ownerProducer);

            if (!string.IsNullOrWhiteSpace(@operator))
                request.AddParameter("operator", @operator);

            var response = Client.Execute<DownloadSummary>(request);

            return response.Data;
        }
    }
}