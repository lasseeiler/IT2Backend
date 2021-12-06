using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IT2_backend.Classes
{
    public class PushNotification
    {
        public static string userHash
        {
            get; set;
        }
        public static string tokenHash
        {
            get; set;
        }
        public void SendNotification()
        {
            try
            { 
                var strPostData = String.Format("token={0}&user={1}&message={2}&url=http%3A%2F%2Fwebinterface.il-torrefattore.dk%2F&url_title=G%C3%A5%20til%20ristning", tokenHash, userHash, "En ny ristning er startet!");
            
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddHeader("cache-control", "no-cache");
                request.AddParameter("application/x-www-form-urlencoded", strPostData, ParameterType.RequestBody);

                var client = new RestClient("https://api.pushover.net/1/messages.json");
                client.Timeout = 2000;
                client.Execute(request);
            }
            catch(Exception e)
            {
            }
        }
    }
    
}