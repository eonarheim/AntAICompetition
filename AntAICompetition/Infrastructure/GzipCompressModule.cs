using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Web;

namespace AntAICompetition.Infrastructure
{
    public class GzipCompressModule : IHttpModule
    {
        public void Init(HttpApplication application)
        {

            application.BeginRequest += (new EventHandler(this.Application_BeginRequest));
        }

        // Your BeginRequest event handler.

        private void Application_BeginRequest(Object source, EventArgs e)
        {

            HttpContext context = HttpContext.Current;
            context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress);

            HttpContext.Current.Response.AppendHeader("Content-encoding", "gzip"); HttpContext.Current.Response.Cache.VaryByHeaders["Accept-encoding"] = true;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}