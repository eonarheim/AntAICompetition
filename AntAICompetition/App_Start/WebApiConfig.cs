using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using AntAICompetition.Infrastructure;
using Microsoft.AspNet.WebApi.MessageHandlers.Compression;
using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Compressors;

namespace AntAICompetition
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "GameApi",
                routeTemplate: "api/{controller}/{id}/{action}/{authToken}",
                defaults: new { id = RouteParameter.Optional , authToken = RouteParameter.Optional}
            );
            var jsonFormatter = new JsonMediaTypeFormatter();
            //optional: set serializer settings here
            config.Services.Replace(typeof(IContentNegotiator), new JsonContentNegotiator(jsonFormatter));

            GlobalConfiguration.Configuration.MessageHandlers.Insert(0, new ServerCompressionHandler(1028, new GZipCompressor(), new DeflateCompressor()));
        }

        
    }
}
