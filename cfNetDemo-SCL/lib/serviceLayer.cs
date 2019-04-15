using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using System.Linq;

namespace cfNetDemo.lib
{
    public class serviceLayer
    {
        /* Service Layer module to interact with B1 Data */
        /* Server Configuration and User Credentials set as environment variables */

        private static CookieContainer cookies;
        private static HttpClientHandler handler;
        private static HttpClient client;
        private static String SessionId;
        
        private static Uri SLServer = new Uri(Environment.GetEnvironmentVariable("B1_SERVER_ENV") + ":"
                            + Environment.GetEnvironmentVariable("B1_SLPORT_ENV")
                            + Environment.GetEnvironmentVariable("B1_SLPATH_ENV"));
        private static int ItemGroupCode = 108; // just for filtering
        

        public serviceLayer()
        {
            /* Constructor connect to SL */
            cookies = new CookieContainer();
            handler = new HttpClientHandler();
            handler.CookieContainer = cookies;
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            client = new HttpClient(handler);
            client.BaseAddress = SLServer;
            

            Task<string> result = Connect();

            dynamic finalResult = JsonConvert.DeserializeObject(result.Result);
            if (finalResult != null)
            {
                SessionId = finalResult.SessionId;
            }else {
                SessionId = "Not Connected to SL";
            }
           
            
                     
        }
        public static string getSessionId()
        {
            return SessionId;
        }
        public static Models getItemsList()
        {
            Task<string> result = getItems();
            var odata = JsonConvert.DeserializeObject<Models>(result.Result);
            return odata;
        }

        public static async Task<bool> postActivity(string msg)
        {
            var data = new Dictionary<string, string>{
                    { "ActivityDate", System.DateTime.Now.Year.ToString() + "-" + System.DateTime.Now.Month.ToString().PadLeft(2, '0') + "-" + System.DateTime.Now.Day.ToString().PadLeft(2, '0')   },
                    { "ActivityTime", System.DateTime.Now.AddHours(2).Hour.ToString().PadLeft(2, '0') + ":" + System.DateTime.Now.Minute.ToString().PadLeft(2, '0') + ":00" },
                    { "Details",msg}
                };

            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            try
            {
                cookies.Add(SLServer, new Cookie("B1SESSION", SessionId));
            }
            catch (Exception ex)
            { }
            

            HttpResponseMessage response = await client.PostAsync("Activities", content);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Error Connecting to Service Layer");
                Console.WriteLine("Response Message Header \n\n" + response.Content.Headers + "\n");
                Console.WriteLine("Response Message Header \n\n" + response.Content.ReadAsStringAsync() + "\n");
                return false;
            }

            return true;

        }

        private static async Task<string> Connect()
        {
            var data = new Dictionary<string, string>{
                    { "UserName", Environment.GetEnvironmentVariable("B1_USER_ENV")},
                    { "Password", Environment.GetEnvironmentVariable("B1_PASS_ENV")},
                    { "CompanyDB",Environment.GetEnvironmentVariable("B1_COMP_ENV")}
                };

            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("Login", content);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Error Connecting to Service Layer");
                Console.WriteLine("Response Message Header \n\n" + response.Content.Headers + "\n");
                Console.WriteLine("Response Message Header \n\n" + response.Content.ReadAsStringAsync() + "\n");
                return "";
            }              
            var responseString = await response.Content.ReadAsStringAsync();

           var responseCookies = cookies.GetCookies(SLServer).Cast<Cookie>();
           return responseString;
        }

        private static async Task<string> getItems()
        {
            String itemEndPoint = "Items?$select=ItemCode,ItemName,"
                                    + "QuantityOnStock,QuantityOrderedFromVendors,QuantityOrderedByCustomers"
                                    + "&$filter=ItemsGroupCode%20eq%20" + ItemGroupCode;

            cookies.Add(SLServer, new Cookie("B1SESSION", SessionId));

            HttpResponseMessage response = await client.GetAsync(itemEndPoint);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Error Retrieving Items");
                Console.WriteLine("Response Message Header \n\n" + response.Content.Headers + "\n");
                Console.WriteLine("Response Message Header \n\n" + response.Content.ReadAsStringAsync() + "\n");
                return "";
            }

            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }


    }
}
