using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
