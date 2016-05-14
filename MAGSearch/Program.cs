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

    public class Program
    {
        static string cquery = "Id,RId,F.FId,C.CId,J.JId,AA.AfId,AA.AuId";
        static string rquery = "Id,AA.AuId";
        static string irquery = "Id,RId";

        static void Main(string[] args)
        {
            long id1 = 2126125555;
            long id2 = 2153635508;

            int startTime = Environment.TickCount;
            Console.WriteLine(solve(id1, id2));
            int endTime = Environment.TickCount;
            Console.WriteLine(endTime - startTime + "ms");

        }

        public static string solve(long id1, long id2)
        {
            Answer ans = new Answer();
            ans.start = id1;
            ans.end = id2;
            var q1 = AllDeserial(MakeRequest("Id=" + id1, cquery, 1, 0));
            var q2 = AllDeserial(MakeRequest("Id=" + id2, cquery, 1, 0));

            string ret = "Not Implemeted";
            if (q1[0].AA.Count == 0)
            {
                if (q2[0].AA.Count == 0)
                {
                    Console.WriteLine("auid->auid");
                    ret = auid2auid(id1, id2, ans);
                }
                else
                {
                    Console.WriteLine("auid->id");
                    ret = auid2id(id1, id2, ans);
                }
            }
            else
            {
                if (q2[0].AA.Count == 0)
                {
                    Console.WriteLine("id->aauid");
                    ret = id2auid(id1, id2, ans);
                }
                else
                {
                    Console.WriteLine("id->id");
                    ret = id2id(id1, id2, ans);
                }
            }
            Console.WriteLine(ans.count());
            return ret;
        }

        static string id2id(long id1, long id2, Answer ans)
        {
            // get information about start and destination
            var q1 = AllDeserial(MakeRequest("Id=" + id1, cquery, 1, 0));
            var q2 = AllDeserial(MakeRequest("Id=" + id2, cquery, 1, 0));

            // 1-hop
            if (Paper.hasLinkRId(q1[0], q2[0])) ans.add1Hop();

            // 2-hop
            ans.add2Hop(id_id_2Hop(q1[0], q2[0]));

            // 3-hop
            id_id_3Hop(q1[0], q2[0], ans);

            return ans.toJson();
        }

        static string id2auid(long id1, long id2, Answer ans)
        {
            // get information about start and destination
            var q1 = AllDeserial(MakeRequest("Id=" + id1, cquery, 1, 0));


            // 1-hop
            if (Paper.hastheAAuId(q1[0], id2)) ans.add1Hop();


            // 2-hop
            ans.add2Hop(id1_auid_hop2(q1[0], id2));

            // 3-hop
            id1_auid_hop3(q1[0], id2, ans);

            return ans.toJson();
        }

        static string auid2id(long id1, long id2, Answer ans)
        {
            int startTime = Environment.TickCount;
            // get information about start and destination
            var q1 = AllDeserial(MakeRequest("Composite(AA.AuId=" + id1 + ")", cquery, 1, 0));
            var q2 = AllDeserial(MakeRequest("Id=" + id2, cquery, 1, 0));


            foreach (var v in q1)
            {
                if (v.Id == id2) ans.add1Hop();
                var mylist = new List<long>();
                mylist = id_id_2Hop(v, q2[0]);
                foreach (var lis in mylist)
                {
                    ans.add3Hop(v.Id, lis);
                }
                if (Paper.hasLinkRId(v, q2[0])) ans.add2Hop(v.Id);
            }


            var list = new List<long>();
            foreach (var r in q1[0].AA)
            {
                if (r.AuId == id2 && r.AfId > 0) list.Add(r.AfId);
            }
            foreach (var r in q2)
            {
                foreach (var i in r.AA)
                {
                    foreach (var j in list)
                    {
                        if (j == i.AfId) ans.add3Hop(i.AfId, i.AuId);
                    }
                }
            }
            foreach (var v in q1)
            {
                foreach (var r in v.Rid)
                {
                    var q3 = AllDeserial(MakeRequest("Id=" + r, cquery, 1, 0));
                    if (Paper.hastheAAuId(q3[0], id2))
                        ans.add3Hop(v.Id, r);
                }

            }

            return ans.toJson();
        }

        static string auid2auid(long id1, long id2, Answer ans)
        {

            // get information about start and destination
            var q1 = AllDeserial(MakeRequest("Composite(AA.AuId=" + id1 + ")", cquery, 1, 0));
            var q2 = AllDeserial(MakeRequest("Composite(AA.AuId=" + id2 + ")", cquery, 1, 0));

            var list = new List<long>();
            foreach (var r in q1[0].AA)
            {
                if (r.AuId == id1 && r.AfId > 0) list.Add(r.AfId);
            }
            foreach (var v in q2[0].AA)
            {
                if (v.AuId == id2)
                    foreach (var r in list)
                    {
                        if (v.AfId == r) { ans.add2Hop(r); break; }
                    }
            }

            return ans.toJson();
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
        static void id1_auid_hop3(Paper p1, long id2, Answer ans)
        {
            var q2 = AllDeserial(MakeRequest("Composite(AA.AuId=" + id2 + ")", cquery, 10000, 0));
            var list = new List<long>();
            foreach (var v in q2)
            {
                foreach (var r in v.AA)
                {
                    if (r.AuId == id2 && r.AfId > 0) list.Add(r.AfId);
                }
            }

            foreach (var r in p1.AA)
            {
                foreach (var t in list)
                {
                    if (r.AfId == t) ans.add3Hop(r.AuId, r.AfId);
                }

            }
            foreach (var r in q2)
            {
                //if (r.Id == p1.Id) continue;
                var list2 = id_id_2Hop(p1, r);

                foreach (var tmp in list2)
                {
                    ans.add3Hop(tmp, r.Id);
                }
            }
            foreach (var v in p1.Rid)
            {
                var q = AllDeserial(MakeRequest("Id=" + v, cquery, 1, 0));
                foreach (var r in q[0].Rid)
                {
                    var q1 = AllDeserial(MakeRequest("Id=" + r, cquery, 1, 0));
                    if (Paper.hastheAAuId(q1[0], id2))
                        ans.add3Hop(v, r);
                }
            }
        }
        static List<long> id_id_2Hop(Paper p1, Paper p2)
        {
            var list = new List<long>();
            if (p1.CId == p2.CId && p1.CId != 0) list.Add(p1.CId);   //id1->CId->id2
            if (p1.JId == p2.JId && p1.JId != 0) list.Add(p1.JId);   //id1->JId->id2
            list.AddRange(Paper.hasLinkFId(p1, p2));                 //id1->FId->id2
            list.AddRange(Paper.hasLinkAAuId(p1, p2));               //id1->AAuId->id2
            return list;
        }

        static void id_id_3Hop(Paper p1, Paper p2, Answer ans)
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

            var p2rid = AllDeserial(MakeRequest("RId=" + p2.Id, cquery, 100000, 0));
            foreach (var v in p1.Rid)
            {
                var q = AllDeserial(MakeRequest("Id=" + v, cquery, 1, 0));
                var l = id_id_2Hop(q[0], p2);
                if (l.Count > 0) ans.add3Hop(v, l);

                //id1->id3->id2
                if (q[0].Rid.Contains(p2.Id)) ans.add2Hop(v);

                //id1->id3->id4->id2
                foreach(var id4 in p2rid)
                {
                    if (q[0].Rid.Contains(id4.Id))
                        ans.add3Hop(v, id4.Id);
                }

            }
        }

        static JObject MakeRequest(string expr, string attr, int count, int offset)
        {
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
