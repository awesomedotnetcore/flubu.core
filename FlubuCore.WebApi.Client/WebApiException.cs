﻿using System;
using System.Collections.Generic;
using System.Net;

namespace FlubuCore.WebApi.Client
{
    public class WebApiException : Exception
    {
        public WebApiException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public string WebApiStackTrace { get; set; }

        public List<string> Logs { get; set; }
    }
}
