﻿using System.Collections.Generic;
using System.Web;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.CustomPages;

namespace kCura.IntegrationPoints.Web
{
    public class SessionService : ISessionService
    {
        public const string SESSION_KEY = "__WEB_SESSION_KEY__";

        public static ISessionService Session
        {
            get
            {
                ISessionService session = null;
                if (HttpContext.Current.Session != null)
                {
                    session = HttpContext.Current.Session[SESSION_KEY] as ISessionService;
                }
                if (session == null)
                {
                    session = new SessionService();
                    if (HttpContext.Current.Session != null)
                    {
                        HttpContext.Current.Session[SESSION_KEY] = session;
                    }
                }
                return session;
            }
        }

        public SessionService()
        {
            Fields = new Dictionary<string, IEnumerable<FieldMap>>();
        }

        public int WorkspaceID
        {
            get
            {
                return ConnectionHelper.Helper().GetActiveCaseID();
            }
        }

        public int UserID
        {
            get
            {
                return ConnectionHelper.Helper().GetAuthenticationManager().UserInfo.ArtifactID;
            }
        }

        public int WorkspaceUserID
        {
            get
            {
                return ConnectionHelper.Helper().GetAuthenticationManager().UserInfo.WorkspaceUserArtifactID;
            }
        }


        public Dictionary<string, IEnumerable<FieldMap>> Fields { get; set; }
    }
}