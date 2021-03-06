﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FlubuCore.WebApi.Client.Attributes;
using FlubuCore.WebApi.Model;
using Newtonsoft.Json;

namespace FlubuCore.WebApi.Client
{
    public class WebApiClient : RestClient, IWebApiClient
    {
        public WebApiClient(HttpClient client)
            : base(client)
        {
        }

        [Post("api/scripts/execute")]
        public async Task<ExecuteScriptResponse> ExecuteScriptAsync(ExecuteScriptRequest request)
        {
           return await SendAsync<ExecuteScriptResponse>(request);
        }

        public async Task UploadScriptAsync(UploadScriptRequest request)
        {
            if (!File.Exists(request.FilePath))
            {
                return;
            }

            using (var content = new MultipartFormDataContent())
            {
                ////todo investigate why one content has to be added.
                content.Add(new ByteArrayContent(new byte[0]), "fake");

                var stream = new FileStream(request.FilePath, FileMode.Open);
                string fileName = Path.GetFileName(request.FilePath);
                content.Add(new StreamContent(stream), fileName, fileName);

                Client.DefaultRequestHeaders.Authorization = !string.IsNullOrEmpty(Token)
                    ? new AuthenticationHeaderValue("Bearer", Token)
                    : null;
                var response = await Client.PostAsync(new Uri(string.Format("{0}api/scripts/upload", WebApiBaseUrl)),
                    content);

                await GetResponse<Void>(response);
            }
        }

        public async Task UploadPackageAsync(UploadPackageRequest request)
        {
            FileInfo[] filesInDir;
            DirectoryInfo directoryInWhichToSearch = new DirectoryInfo(request.DirectoryPath);
            if (!string.IsNullOrEmpty(request.PackageSearchPattern))
            {
                filesInDir = directoryInWhichToSearch.GetFiles(request.PackageSearchPattern);
            }
            else
            {
                filesInDir = directoryInWhichToSearch.GetFiles();
            }

            if (filesInDir.Length == 0)
            {
                return;
            }

            using (var content = new MultipartFormDataContent())
            {
                ////todo investigate why one content has to be added.
                var json = JsonConvert.SerializeObject(request);
                content.Add(new StringContent(json), "request");
                foreach (var file in filesInDir)
                {
                    var stream = new FileStream(file.FullName, FileMode.Open);
                    string fileName = Path.GetFileName(file.FullName);
                    content.Add(new StreamContent(stream), fileName, fileName);
                }

                Client.DefaultRequestHeaders.Authorization = !string.IsNullOrEmpty(Token)
                    ? new AuthenticationHeaderValue("Bearer", Token)
                    : null;
                var response = await Client.PostAsync(new Uri(string.Format("{0}api/packages", WebApiBaseUrl)), content);

                await GetResponse<Void>(response);
            }
        }

        [Delete("api/packages")]
        public async Task DeletePackagesAsync(CleanPackagesDirectoryRequest request)
        {
            await SendAsync(request);
        }

        [Post("api/Auth")]
        public async Task<GetTokenResponse> GetToken(GetTokenRequest request)
        {
            return await SendAsync<GetTokenResponse>(request);
        }

        [Get("api/HealthCheck")]
        public async Task HealthCheckAsync()
        {
            await SendAsync();
        }

        [Post("api/reports/download")]
        public async Task<Stream> DownloadReportsAsync(DownloadReportsRequest request)
        {
           return await GetStreamAsync(request);
        }

        [Delete("api/reports/download")]
        public async Task CleanReportsDirectoryAsync(CleanReportsDirectoryRequest request)
        {
            await SendAsync(request);
        }
    }
}
