using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AarkNotify.Helper
{
    public class HttpHelper
    {
        public static async Task<string> SendFlurlRequest(string url, string method, object headers, string requestBody)
        {
            try
            {
                var request = url.WithHeaders(headers); // 设置请求头

                // 根据方法发送不同类型的请求
                if (method == "POST")
                {
                    var response = await request.PostJsonAsync(requestBody).ReceiveString();
                    return response;
                }
                else if (method == "PUT")
                {
                    var response = await request.PutJsonAsync(requestBody).ReceiveString();
                    return response;
                }
                else
                {
                    var response = await request.GetStringAsync();
                    return response;
                }
            }
            catch (FlurlHttpException ex)
            {
                if (ex.Call.Response != null)
                {
                    var responseBody = await ex.Call.Response.ResponseMessage.Content.ReadAsStringAsync();
                    return responseBody;
                }
                return ex.Message;
            }
        }

        public static async Task<T> SendHttpRequest<T>(string url, string method, Dictionary<string, string> headers, string requestBody)
        {
            try
            {
                var clientHandler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                };
                var client = new HttpClient(clientHandler);
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://api-gateway.guahaoe.com/order/shiftcase/listbymonth.json"),
                    Content = new StringContent("{\"hospitalId\":\"4d2ea0b8-7c1f-41c6-8e5e-69eb20c5a0f0000\",\"hospDeptId\":\"baa37279-7162-421d-ba0a-4e1f4b2a2213000\",\"doctorId\":\"46b92102-20f2-460a-a8c1-81b1330fabbd000\"}")
                    {
                        Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                    }
                };
                foreach (var header in headers)
                {
                    try
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                    catch
                    {
                        continue;
                    }
                }
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(body);
                }
            }
            catch
            {
                throw new Exception("请求失败");
            }
        }
    }
}
