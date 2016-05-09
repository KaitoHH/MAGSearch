using System;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;

namespace MAGSearch
{
    public class SearchResult
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }
    }

    public class Paper
    {
        public long Id { get; set; }

        public List<long> Rid { get; set; }

        public List<_AA> AA { get; set; }
        public class _AA
        {
            public long AuId { get; set; }
            public long AfId { get; set; }
        }
        public List<long> FId { get; set; }
        public long CId { get; set; }
        public long JId { get; set; }

        public void show()
        {
            Console.WriteLine("Id:{0},CId:{1},JId:{2}", Id, CId, JId);
            Console.WriteLine("Rid:");
            foreach (var r in Rid)
                Console.WriteLine(r);
            Console.WriteLine("AA:");
            foreach (var a in AA)
                Console.WriteLine("AA.AfId:{0},AA.AuId{1}", a.AfId, a.AuId);
            Console.WriteLine("F.Fid:");
            try
            {
                foreach (var f in FId)
                    Console.WriteLine(f);
            }
            catch { }
        }
    }
    class Program
    {
        static List<Paper> queue = new List<Paper>();
        static bool req_event;
        static void Main(string[] args)
        {
            MakeRequest("Y=2016", "Id,RId,F.FId,C.CId,J.JId,AA.AfId,AA.AuId", 10000, 0);
            //while (req_event) ;
            Console.WriteLine(queue.Count);
        }
        static async void MakeRequest(string expr, string attr, int count, int offset)
        {
            req_event = true;
            queue.Clear();
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request parameters
            queryString["expr"] = expr;
            queryString["model"] = "latest";
            queryString["attributes"] = attr;
            queryString["count"] = count.ToString();
            queryString["offset"] = offset.ToString();

            var uri = "https://oxfordhk.azure-api.net/academic/v1.0/evaluate?" + queryString + " &subscription-key=f7cc29509a8443c5b3a5e56b0e38b5a6";

            /*
            var response = await client.GetAsync(uri);
            var body = response.Content;

            //Console.WriteLine(uri);
            var str = await body.ReadAsStringAsync();
            //Console.WriteLine(str);
            */
            
            HttpWebRequest myReq = (HttpWebRequest)HttpWebRequest.Create(uri);
            HttpWebResponse HttpWResp = (HttpWebResponse)myReq.GetResponse();
            Stream myStream = HttpWResp.GetResponseStream();
            StreamReader sr;
            string str;
            using (sr = new StreamReader(myStream, Encoding.UTF8))
            {
                str = sr.ReadToEnd();
            }
            sr.Close();
            HttpWResp.Close();
            myReq.Abort();
            
            //Console.WriteLine(str);
            Console.WriteLine("抓取完成");
            JObject ret = JObject.Parse(str);
            //Console.WriteLine(ret.ToString());

            IList<JToken> results = ret["entities"].Children().ToList();

            foreach (JToken result in results)
            {
                Paper p = new Paper();
                p.Id = long.Parse(result["Id"].ToString());
                p.Rid = JsonConvert.DeserializeObject<List<long>>(result["RId"].ToString());
                p.AA = JsonConvert.DeserializeObject<List<Paper._AA>>(result["AA"].ToString());
                try { p.JId = long.Parse(result["J"]["JId"].ToString()); } catch { }
                try { p.CId = long.Parse(result["C"]["CId"].ToString()); } catch { }

                try
                {
                    var fid = result["F"].Children().ToList();
                    p.FId = new List<long>();
                    foreach (var ffid in fid)
                    {
                        //Console.WriteLine(ffid);
                        string s = ffid["FId"].ToString();
                        //Console.WriteLine(s);
                        p.FId.Add(long.Parse(s));
                    }
                }
                catch { }
                queue.Add(p);
                //p.show();
            }
            Console.WriteLine("解析完成");
            req_event = false;
        }
    }
}
