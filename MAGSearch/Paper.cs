using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAGSearch
{
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
}
