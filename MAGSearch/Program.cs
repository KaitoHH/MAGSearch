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
    public class Answer
    {
        public long start { get; set; }
        public long end { get; set; }
        HashSet<List<long>> ret = new HashSet<List<long>>();
        public void add1Hop()
        {
            ret.Add(new List<long>(new long[] { start, end }));
        }
        public void add2Hop(long id1)
        {
            ret.Add(new List<long>(new long[] { start, id1, end }));
        }
        public void add3Hop(long id1, long id2)
        {
            var t = new List<long>(new long[] { start, id1, id2, end });
            if (!ret.Contains(t))
                ret.Add(t);
        }
        public void add2Hop(List<long> l)
        {
            foreach (var ll in l)
            {
                add2Hop(ll);
            }
        }
        public void add3Hop(long id, List<long> l)
        {
            foreach (var ll in l)
            {
                add3Hop(id, ll);
            }
        }
        public string toJson()
        {
            return JsonConvert.SerializeObject(ret);
        }

        public int count()
        {
            return ret.Count;
        }
    }

    public class Paper
    {
        public long Id { get; set; }

        public List<long> Rid { get; set; } = new List<long>();

        public List<_AA> AA { get; set; } = new List<_AA>();
        public class _AA
        {
            public long AuId { get; set; }
            public long AfId { get; set; }
        }
        public List<long> FId { get; set; } = new List<long>();
        public long CId { get; set; }
        public long JId { get; set; }
        public int hops { get; set; }

        static public void show(Paper p)
        {
            Console.WriteLine("Id:{0},CId:{1},JId:{2}", p.Id, p.CId, p.JId);
            Console.WriteLine("Rid:");
            foreach (var r in p.Rid)
                Console.WriteLine(r);

            Console.WriteLine("AA:");
            foreach (var a in p.AA)
                Console.WriteLine("AA.AfId:{0},AA.AuId{1}", a.AfId, a.AuId);

            Console.WriteLine("F.Fid:");
            foreach (var f in p.FId)
                Console.WriteLine(f);
        }
        static public bool hasLinkRId(Paper p1, Paper p2)
        {
            foreach (var r in p1.Rid)
            {
                if (r == p2.Id)
                    return true;
            }
            return false;
        }
        static public List<long> hasLinkFId(Paper p1, Paper p2)
        {
            var ans = new List<long>();
            HashSet<long> hs = new HashSet<long>();
            if (p1.FId.Count > 0 && p2.FId.Count > 0)
            {
                foreach (var r in p1.FId)
                {
                    hs.Add(r);
                }
                foreach (var r in p2.FId)
                {
                    if (hs.Contains(r))
                    {
                        ans.Add(r);
                    }
                }
            }
            return ans;
        }
        static public List<long> hasLinkAAuId(Paper p1, Paper p2)
        {
            var ans = new List<long>();
            HashSet<long> hs = new HashSet<long>();
            if (p1.AA.Count > 0 && p2.AA.Count > 0)
            {
                foreach (var r in p1.AA)
                {
                    hs.Add(r.AuId);
                }
                foreach (var r in p2.AA)
                {
                    if (hs.Contains(r.AuId))
                    {
                        ans.Add(r.AuId);
                    }
                }
            }
            return ans;
        }

        static public string queryComposite(string q1, long id1, string q2, long id2)
        {
            return "And(Composite(" + q1 + " = " + id1 + "), " + q2 + " = " + id2 + ")";
        }



    }
    class Program
    {
        static bool req_event;
        static string cquery = "Id,RId,F.FId,C.CId,J.JId,AA.AfId,AA.AuId";
        static string rquery = "Id,AA.AuId";
        static string irquery = "Id,RId";

        static void Main(string[] args)
        {
            Answer ans = new Answer();
            long id1 = 2030985472;
            long id2 = 2133644056;

            ans.start = id1;
            ans.end = id2;

            // get information about start and destination
            var q1 = AllDeserial(MakeRequest("Id=" + id1, cquery, 1, 0));
            var q2 = AllDeserial(MakeRequest("Id=" + id2, cquery, 1, 0));

            // 1-hop
            if (Paper.hasLinkRId(q1[0], q2[0])) ans.add1Hop();

            //Console.WriteLine("开始搜索");
            // 2-hop
            //Console.WriteLine("开始搜索CId");
            if (q1[0].CId == q2[0].CId && q1[0].CId != 0) ans.add2Hop(q1[0].CId);   //id1->CId->id2
            //Console.WriteLine(ans.toJson());
            //Console.WriteLine("开始搜索JId");
            if (q1[0].JId == q2[0].JId && q1[0].JId != 0) ans.add2Hop(q1[0].JId);   //id1->JId->id2
            //Console.WriteLine(ans.toJson());
            //Console.WriteLine("开始搜索FId");
            ans.add2Hop(Paper.hasLinkFId(q1[0], q2[0]));                            //id1->FId->id2
            //Console.WriteLine(ans.toJson());
            //Console.WriteLine("开始搜索AAuId");
            ans.add2Hop(Paper.hasLinkAAuId(q1[0], q2[0]));                          //id1->AAuId->id2
            //Console.WriteLine(ans.toJson());

            int startTime = Environment.TickCount;
            // binary-3-hop
            solve3Hop(q1[0], q2[0], ans);
            //getCId(q2[0], q1[0], ans);
            int endTime = Environment.TickCount;

            int runTime = endTime - startTime;

            Console.WriteLine(ans.toJson());
            Console.WriteLine(ans.count());
            Console.WriteLine("Time: " + runTime);
        }
        static void solve3Hop(Paper p1, Paper p2, Answer ans)
        {

            //CId
            if (p1.CId != 0)
            {
                var cidqst = MakeRequest(Paper.queryComposite("C.CId", p1.CId, "RId", p2.Id), "Id", 10000, 0);
                var qcid = IdDeserial(cidqst);
                ans.add3Hop(p1.CId, qcid);
            }

            //JId
            if (p1.JId != 0)
            {
                var jidqst = MakeRequest(Paper.queryComposite("J.JId", p1.JId, "RId", p2.Id), "Id", 10000, 0);
                var qjid = IdDeserial(jidqst);
                ans.add3Hop(p1.JId, qjid);
            }

            //FId
            foreach (var v in p1.FId)
            {
                var fidqst = MakeRequest(Paper.queryComposite("F.FId", v, "RId", p2.Id), "Id", 10000, 0);
                var qfid = IdDeserial(fidqst);
                ans.add3Hop(v, qfid);
            }

            //AA.AuId
            foreach (var v in p1.AA)
            {
                var auidqst = MakeRequest(Paper.queryComposite("AA.AuId", v.AuId, "RId", p2.Id), "Id", 10000, 0);
                var auid = IdDeserial(auidqst);
                ans.add3Hop(v.AuId, auid);
            }
        }
        static JObject MakeRequest(string expr, string attr, int count, int offset)
        {
            req_event = true;
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request parameters
            queryString["expr"] = expr;
            queryString["model"] = "latest";
            queryString["attributes"] = attr;
            queryString["count"] = count.ToString();
            queryString["offset"] = offset.ToString();

            var uri = "https://oxfordhk.azure-api.net/academic/v1.0/evaluate?" + queryString + "&subscription-key=f7cc29509a8443c5b3a5e56b0e38b5a6";

            //Console.WriteLine(uri);
            /*
            var response = await client.GetAsync(uri);
            var body = response.Content;

            var str = await body.ReadAsStringAsync();
            
            */
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(uri);
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
            //Console.WriteLine("抓取完成");
            JObject ret = JObject.Parse(str);
            //Console.WriteLine(ret.ToString());

            //req_event = false;
            return ret;
        }

        static public List<long> IdDeserial(JObject json)
        {
            var ret = new List<long>();
            IList<JToken> results = json["entities"].Children().ToList();

            foreach (JToken result in results)
            {

                try
                {
                    long cur;
                    cur = long.Parse(result["Id"].ToString());
                    ret.Add(cur);
                }
                catch { }
            }
            //Console.WriteLine("解析完成");
            return ret;
        }

        static public List<Paper> AllDeserial(JObject json)
        {
            var ret = new List<Paper>();
            IList<JToken> results = json["entities"].Children().ToList();

            foreach (JToken result in results)
            {
                Paper p = new Paper();
                try { p.Id = long.Parse(result["Id"].ToString()); } catch { }
                try { p.Rid = JsonConvert.DeserializeObject<List<long>>(result["RId"].ToString()); } catch { }

                try { p.AA = JsonConvert.DeserializeObject<List<Paper._AA>>(result["AA"].ToString()); } catch { }
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
                ret.Add(p);
                //Paper.show(p);
            }
            //Console.WriteLine("解析完成");
            return ret;
        }
    }
}
