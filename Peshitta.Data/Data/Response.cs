using System;
using System.Collections.Generic;
using System.Net;

namespace Peshitta.Data.Data
{
    /// <summary>
    /// KitabDB response
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Response<T>
    {
        public Response(int totalCount, string eTAG, DateTime lastDateTime, IEnumerable<T> data)
        {
            this.TotalCount = totalCount;
            this.eTAG = eTAG;
            this.LastDateTime = lastDateTime;
            this.Data = data;
            StatusCode = HttpStatusCode.OK;
        }
        public HttpStatusCode StatusCode { get; set; }
        public int TotalCount { get;}
        public string eTAG { get; }

        public DateTime LastDateTime { get;  }

        public IEnumerable<T> Data { get; }
    }
}
