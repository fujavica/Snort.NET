using System;

namespace snortdb
{
    public class Ref
    {
        public Ref()
        {
        }

        public Ref(int ref_id, int ref_system_id, string ref_tag)
        {
            this.ref_id = ref_id;
            this.ref_system_id = ref_system_id;
            this.ref_tag = ref_tag;

        }

        public int ref_id { get; set; }
        public int ref_system_id { get; set; }
        public string ref_tag { get; set; }
    }
}


