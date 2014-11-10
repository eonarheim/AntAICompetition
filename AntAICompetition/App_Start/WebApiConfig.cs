using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

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
        }
    }
}
