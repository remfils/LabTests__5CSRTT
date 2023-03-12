using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabTests__5CSRTT.Models
{
    public class Message_5CSRTT
    {
        public DateTime RegisterDate;

        public byte aisle;
        public byte category;
        public byte address;
        public byte value_1 = 0x00;
        public byte value_2 = 0x00;
        public byte value_3 = 0x00;

        public Message_5CSRTT()
        {
        }


        // example of bytelist:
        // 0xaa     0xbb	 0x01	0x21	0x00	 0x00   0x00	 0x00	 0xcc	 0xdd
        // skip     skip     aisle  cat     addr     val    val      val     skip    skip                   
        public Message_5CSRTT(List<byte> byteList)
        {
            var len = byteList.Count;
            if (len < 6)
            {
                return;
            }

            int i = 0;
            i++; i++;

            aisle = byteList[i++];
            category = byteList[i++];
            address = byteList[i++];
            value_1 = byteList[i++];
            value_2 = byteList[i++];
            value_3 = byteList[i++];

            // NOTE: how accurate is DateTime realy?
            RegisterDate = DateTime.Now;

            // NOTE: some sanity checks
            if (value_1 == 0xcc || value_1 == 0xdd) throw new Exception("value_1 had to be skiped, wrong byte list size");
            if (value_2 == 0xcc || value_2 == 0xdd) throw new Exception("value_2 had to be skiped, wrong byte list size");
            if (value_3 == 0xcc || value_3 == 0xdd) throw new Exception("value_3 had to be skiped, wrong byte list size");
        }

    }
}
