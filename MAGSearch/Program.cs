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
        public int hops { get; set; }
        public Paper()
        {
            Rid = new List<long>();
            AA = new List<_AA>();
            FId = new List<long>();
        }
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
        static public bool hastheAAuId(Paper p1, long p2)
        {
            if (p1.AA.Count > 0)
            {
                foreach (var r in p1.AA)
                {
                    if (r.AuId == p2) return true;
                }
            }
            return false;
        }
        static public bool hastheAfId(Paper p1, long p2)
        {
            if (p1.AA.Count > 0)
            {
                foreach (var r in p1.AA)
                {
                    if (r.AfId == p2) return true;
                }
            }
            return false;
        }

        static public string queryComposite(string q1, long id1, string q2, long id2)
        {
            return "And(Composite(" + q1 + " = " + id1 + "), " + q2 + " = " + id2 + ")";
        }



    }
    public class Program
    {
        static bool req_event;
        static string cquery = "Id,RId,F.FId,C.CId,J.JId,AA.AfId,AA.AuId";
        static string rquery = "Id,AA.AuId";
        static string irquery = "Id,RId";

        static void Main(string[] args)
        {
            Answer ans = new Answer();
            long id1 = 2063132010;
            long id2 = 2153521822;

            //long id1 = 2018949714;
            //long id2 = 2105005017;
            Console.WriteLine(solve(id1, id2));
            Console.ReadLine();
        }

        public static string solve(long id1, long id2)
        {
            var q1 = AllDeserial(MakeRequest("Id=" + id1, cquery, 1, 0));
            var q2 = AllDeserial(MakeRequest("Id=" + id2, cquery, 1, 0));

            if (q1[0].AA.Count == 0)
            {
                if (q2[0].AA.Count == 0)//auid->auid
                {
                    Console.WriteLine("auid->auid");
                }
                else//auid->id
                {
                    Console.WriteLine("auid->id");
                    return auid2id(id1, id2);
                }
            }
            else
            {
                if (q2[0].AA.Count == 0)//id->aauid
                {
                    Console.WriteLine("id->aauid");
                    return id2aauid(id1, id2);
                }
                else//id->id
                {
                    Console.WriteLine("id->id");
                    return id2id(id1, id2);
                }
            }
            return "";
        }

        static string id2id(long id1, long id2)
        {
            Answer ans = new Answer();
            ans.start = id1;
            ans.end = id2;

            int startTime = Environment.TickCount;
            // get information about start and destination
            var q1 = AllDeserial(MakeRequest("Id=" + id1, cquery, 1, 0));
            var q2 = AllDeserial(MakeRequest("Id=" + id2, cquery, 1, 0));

            // 1-hop
            if (Paper.hasLinkRId(q1[0], q2[0])) ans.add1Hop();

            // 2-hop
            ans.add2Hop(solve2Hop(q1[0], q2[0]));

            // binary-3-hop
            solve3Hop(q1[0], q2[0], ans);

            int endTime = Environment.TickCount;

            int runTime = endTime - startTime;

           
            return ans.toJson();
        }

        static string id2aauid(long id1, long id2)
        {
            Answer ans = new Answer();
            ans.start = id1;
            ans.end = id2;

            int startTime = Environment.TickCount;
            // get information about start and destination
            var q1 = AllDeserial(MakeRequest("Id=" + id1, cquery, 1, 0));


            // 1-hop
            if (Paper.hastheAAuId(q1[0], id2)) ans.add1Hop();
        

            // 2-hop
            ans.add2Hop(id1_auid_hop2(q1[0], id2));

            // 3-hop
            id1_auid_hop3(q1[0], id2, ans);



            int endTime = Environment.TickCount;

            int runTime = endTime - startTime;

           
            return ans.toJson();
        }
        static string auid2id(long id1, long id2)
        {
            Answer ans = new Answer();
            ans.start = id1;
            ans.end = id2;

            int startTime = Environment.TickCount;
            // get information about start and destination
            var q1 = AllDeserial(MakeRequest("Composite(AA.AuId=" + id1 + ")", cquery, 1, 0));
            var q2 = AllDeserial(MakeRequest("Id=" + id2, cquery, 1, 0));
            
        
            foreach (var v in q1)
            {
                if (v.Id == id2) ans.add1Hop();
                ans.add2Hop(solve2Hop(v, q2[0]));
                if (Paper.hasLinkRId(v, q2[0])) ans.add2Hop(v.Id);
            }

            
            // 2-hop
            ans.add2Hop(solve2Hop(q1[0], q2[0]));

            var list = new List<long>();
            foreach (var r in q1[0].AA)
            {
                if (r.AuId == id2) list.Add(r.AfId);
            }
            foreach (var r in q2)
            {
                foreach (var i in r.AA)
                {
                    foreach (var j in list)
                    {
                        if (j == i.AfId) ans.add3Hop(j, i.AuId);
                    }
                }
            }

            int endTime = Environment.TickCount;

            int runTime = endTime - startTime;

        
            return ans.toJson();
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

            foreach (var v in p1.Rid)
            {
                var q = AllDeserial(MakeRequest("Id=" + v, cquery, 1, 0));
                var l = solve2Hop(q[0], p2);
                if (l.Count > 0) ans.add3Hop(v, l);


                if (q[0].Rid.Contains(p2.Id)) ans.add2Hop(v);
                //Console.WriteLine(ans.count());

                // 以下代码完整 然而太慢
                //id1->id3->id4->id2
                /*foreach (var r in q[0].Rid)
                {
                    var ridqst = MakeRequest("And(Id=" + r + ",RId=" + p2.Id + ")", "Id", 1, 0);
                    var rfid = IdDeserial(ridqst);
                    ans.add3Hop(v, rfid);
                    Console.WriteLine(ans.count());
                }*/
            }
        }
        static void id1_auid_hop3(Paper p1, long id2, Answer ans)
        {
            
            var q2 = AllDeserial(MakeRequest("Composite(AA.AuId=" + id2 + ")", cquery, 1, 0));
            long theAfId = 0;
            foreach (var r in q2[0].AA)
            {
                if (r.AuId == id2) theAfId = r.AfId;
            }
             if (theAfId != 0) foreach (var r in p1.AA)
                {
                    if (r.AfId == theAfId && r.AuId != id2) ans.add3Hop(r.AuId, theAfId);
                }
                
            foreach (var r in q2)
            {
                //if (r.Id == p1.Id) continue;
                var list = solve2Hop(p1, r);
                foreach (var tmp in list)
                {
                    
                    ans.add3Hop(tmp, r.Id);
                }
            }
        }
        static List<long> solve2Hop(Paper p1, Paper p2)
        {
            var list = new List<long>();
            if (p1.CId == p2.CId && p1.CId != 0) list.Add(p1.CId);   //id1->CId->id2
            //Console.WriteLine(ans.toJson());
            //Console.WriteLine("开始搜索JId");
            if (p1.JId == p2.JId && p1.JId != 0) list.Add(p1.JId);   //id1->JId->id2
            //Console.WriteLine(ans.toJson());
            //Console.WriteLine("开始搜索FId");
            list.AddRange(Paper.hasLinkFId(p1, p2));                            //id1->FId->id2
            //Console.WriteLine(ans.toJson());
            //Console.WriteLine("开始搜索AAuId");
            list.AddRange(Paper.hasLinkAAuId(p1, p2));                          //id1->AAuId->id2
            return list;
        }
        static List<long> id1_auid_hop2(Paper p1, long id2)
        {
            var list = new List<long>();
            foreach (var r in p1.Rid)
            {
                var q1 = AllDeserial(MakeRequest("Id=" + r, cquery, 1, 0));
                if (Paper.hastheAAuId(q1[0], id2))
                    list.Add(r);

            }
            return list;

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

          // ********//Console.WriteLine(uri);
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
