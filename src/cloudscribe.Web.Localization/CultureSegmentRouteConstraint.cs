﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace cloudscribe.Web.Localization
{
    public class CultureSegmentRouteConstraint : IRouteConstraint
    {
        public CultureSegmentRouteConstraint(bool useSecondSegment = false)
        {
            _useSecondSegment = useSecondSegment;
        }

        private bool _useSecondSegment;

        public bool Match(
            HttpContext httpContext,
            IRouter route,
            string routeKey,
            RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            if (httpContext == null) { return false; }



            string requestFolder = GetStartingSegment(httpContext.Request.Path);

            if (!string.IsNullOrWhiteSpace(requestFolder))
            {
                var logger = httpContext.RequestServices.GetService<ILogger<CultureSegmentRouteConstraint>>();

                var cultureSettingsAccessor = httpContext.RequestServices.GetService<IOptions<RequestLocalizationOptions>>();
                var cultureSettings = cultureSettingsAccessor.Value;
                var found = cultureSettings.SupportedUICultures.Where(x => 
                x.Name.Equals(requestFolder,System.StringComparison.InvariantCultureIgnoreCase) 
                || x.TwoLetterISOLanguageName.Equals(requestFolder, System.StringComparison.InvariantCultureIgnoreCase )
                ).Any();
                
                var isDefaultCulture = cultureSettings.DefaultRequestCulture.UICulture.Name.Equals(requestFolder, System.StringComparison.InvariantCultureIgnoreCase) 
                    || cultureSettings.DefaultRequestCulture.UICulture.TwoLetterISOLanguageName.Equals(requestFolder, System.StringComparison.InvariantCultureIgnoreCase);
                
                
                
                //don't match default culture because we don't want the culture segment in the url for the default culture
                if (found && !isDefaultCulture)
                {
                    logger.LogDebug($"matched culture route constraint for path {httpContext.Request.Path}");

                    return true;
                }

                logger.LogDebug($"did not match culture route constraint for path {httpContext.Request.Path}");
            }


            return false;
        }

        private string GetStartingSegment(string requestPath)
        {
            if (string.IsNullOrEmpty(requestPath)) return requestPath;
            if (!requestPath.Contains("/")) return requestPath;

            var segments = SplitOnCharAndTrim(requestPath, '/');
            if(_useSecondSegment)
            {
                if(segments.Count > 1)
                {
                    return segments[1]; //second segment
                }

                return null; 
            }
            else
            {
                return segments.FirstOrDefault();
            }
            
        }

        private List<string> SplitOnCharAndTrim(string s, char c)
        {
            List<string> list = new List<string>();
            if (string.IsNullOrWhiteSpace(s)) { return list; }

            string[] a = s.Split(c);
            foreach (string item in a)
            {
                if (!string.IsNullOrWhiteSpace(item)) { list.Add(item.Trim()); }
            }


            return list;
        }
    }
}
